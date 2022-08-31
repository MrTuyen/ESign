using Newtonsoft.Json;
using Npgsql;
using OnSign.BusinessLogic;
using OnSign.BusinessLogic.Account;
using OnSign.BusinessLogic.Document;
using OnSign.BusinessLogic.Email;
using OnSign.BusinessObject.Email;
using OnSign.BusinessObject.Sign;
using OnSign.Common.Helpers;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using System.Linq;
using OnSign.Common;
using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Forms;

namespace SendEmailService
{
    public partial class Service : ServiceBase
    {
        private NpgsqlConnection conn = new NpgsqlConnection(ConfigHelper.Instance.GetConnectionStringDS());
        private static System.Timers.Timer timer = new System.Timers.Timer();
        private int timeRepeatSecondsDefault = 1;
        private int timeRepeatHoursDefault = 1;

        public Service()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                this.InitSvcTimer();
                Thread newProcess = new Thread(ListenForNotifications)
                {
                    IsBackground = true
                };
                newProcess.Start();
            }
            catch (Exception ex)
            {
                FileLogger.LogAction($"Lỗi kết nối Trigger Database: {ex}");
            }
        }

        private void InitSvcTimer()
        {
            //Chạy lần đầu, hệ thống sẽ tự động sau 5s
            timer.Elapsed += new ElapsedEventHandler(WorkerProcess);
            timer.Interval = TimeSpan.FromSeconds(timeRepeatSecondsDefault).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();
        }

        private void WorkerProcess(object sender, ElapsedEventArgs e)
        {
            //Set lại thời gian delay giữa các tiến trình
            timer.Interval = timer.Interval = TimeSpan.FromHours(timeRepeatHoursDefault).TotalMilliseconds;
            timer.Stop();
            try
            {
                var lstSign = new List<DocumentSignBO>();
                var emailDatas = new List<EmailDataBO>();
                DocumentBLL documentBLL = new DocumentBLL();
                var stauts_Pending = documentBLL.GetRequestReSend(new FormSearch());
                foreach (var rq in stauts_Pending)
                {
                    var request = documentBLL.GetRequestById(new FormSearch() { ID = rq.ID });
                    //Kiểm tra thời gian chạy luồng hiện tại có bằng = thời gian khởi tạo hay không?
                    if (request.CREATEDATTIME.TimeOfDay.Hours == DateTime.Now.TimeOfDay.Hours)
                    {
                        request.FILEUPLOADS.ForEach((doc) =>
                        {
                            doc.SIGN.OrderBy(x => x.SIGNINDEX).ToList().ForEach((s) =>
                            {
                                lstSign.Add(s);
                            });
                        });

                        var nextSign = lstSign.Where(x => !x.ISSIGNED && !x.ISDECLINED).FirstOrDefault();
                        if (nextSign != null)
                        {
                            var linkViewer = BaseBLL.GenerateLinkViewer(RequestStatus.CHO_KY, nextSign.EMAILASSIGNMENT, request.ID, false, nextSign.SIGNINDEX);
                            var email = request.LISTMAILTO.Where(x => x.EMAIL == nextSign.EMAILASSIGNMENT).FirstOrDefault();
                            string Code = string.Format("{0}.{1}", request.CREATEDBYUSER, request.ID);
                            email = email ?? new MailToBO();
                            var subject = $"{string.Format(Constants.SIGN_SUBJECT, Code, RequestStatus.CHO_KY, " Nhắc lại - " + request.EMAILSUBJECT)} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                            var msg = string.Format(Constants.SIGN_ONE_SENT, request.FULLNAME, request.EMAIL).Replace("@", "&#64;").Replace(".", "&#46;");
                            var logo = email.ISCC ? DocumentLogo.GENERIC : DocumentLogo.REQUEST;
                            var objUser = new AccountBO() { FULLNAME = request.FULLNAME, ID = request.CREATEDBYUSER };
                            documentBLL.MakeListSendMail(request, emailDatas, linkViewer, email, subject, msg, logo, objUser, true);
                        }
                    }
                }
                AccountBLL accountBLL = new AccountBLL();
                var resultAddEmail = accountBLL.AddEmail(emailDatas);
            }
            catch (Exception objMainEx)
            {
                FileLogger.LogAction($"Exception thrown in Timer Service: {objMainEx}");
            }
            timer.Start();
        }

        private void ListenForNotifications()
        {
            ExecuteListenNotify();
            conn.Notification += PostgresNotificationReceived;
            while (true)
            {
                try
                {
                    if (!(conn.State == ConnectionState.Open))
                    {
                        ExecuteListenNotify();
                    }
                    conn.Wait();
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10000);
                    FileLogger.LogAction($"Lỗi kết nối Trigger Database: {ex}");
                }
            }
        }

        private void ExecuteListenNotify()
        {
            conn.Open();
            var listenCommand = conn.CreateCommand();
            listenCommand.CommandText = $"listen email_notify";
            listenCommand.ExecuteNonQuery();
        }

        private void PostgresNotificationReceived(object sender, NpgsqlNotificationEventArgs e)
        {
            string data = e.Payload;
            var emailData = JsonConvert.DeserializeObject<EmailDataBO>(e.Payload);
            if (string.IsNullOrEmpty(emailData.FromEmail))
            {
                emailData.FromName = "Onfinance Sign";
                emailData.FromEmail = ConfigHelper.UsernameEmail;
            }

            AccountBLL accountBLL = new AccountBLL();
            var r1 = accountBLL.GetTemplateByMailType(emailData.EmailType);
            var temp = r1.TEMPLATE;
            switch (r1.EMAILTYPE)
            {
                case Constants.TEMPLATE_MAILTYPE_INVITATION:
                    temp = temp.Replace("{emailData.Subject)}", emailData.Subject);
                    temp = temp.Replace("{emailData.DocumentMessage}", emailData.DocumentMessage);
                    temp = temp.Replace("{emailData.MailName}", emailData.MailName);
                    temp = temp.Replace("{LinkView}", emailData.DocumentLinkViewer);
                    break;
                case Constants.TEMPLATE_MAILTYPE_DOCUMENT:
                    temp = temp.Replace("{emailData.Subject)}", emailData.Subject);
                    temp = temp.Replace("{documentSignImageUrl}", emailData.DocumentLinkLogo);
                    temp = temp.Replace("{emailData.DocumentMessage}", emailData.DocumentMessage);
                    temp = temp.Replace("{emailData.Message}", emailData.Messages);
                    temp = temp.Replace("{emailData.MailName}", emailData.MailName);
                    temp = temp.Replace("{LinkView}", emailData.DocumentLinkViewer);
                    break;
                case Constants.TEMPLATE_MAILTYPE_FORGOT_PASSWORD:
                    temp = temp.Replace("{emaiData.Content}", emailData.Content);
                    break;
            }
            emailData.Content = temp;
            new Thread(() =>
            {
                var result = EmailSender.MailSender(new EmailDataBO
                {
                    MailTo = emailData.MailTo,
                    MailName = emailData.MailName,
                    FromEmail = emailData.FromEmail,
                    FromName = emailData.FromName,
                    Subject = emailData.Subject,
                    Messages = emailData.Messages,
                    Content = emailData.Content
                });
                if (result == true)
                {
                    emailData.IsSent = true;
                    result = accountBLL.UpdateEmail(emailData);
                }
            }).Start();
        }
    }
}