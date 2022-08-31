using Newtonsoft.Json;
using Npgsql;
using OnSign.BusinessLogic;
using OnSign.BusinessLogic.Account;
using OnSign.BusinessLogic.Document;
using OnSign.BusinessLogic.Email;
using OnSign.BusinessLogic.Notifications;
using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Email;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Sign;
using OnSign.Common;
using OnSign.Common.Helpers;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
//using Newtonsoft.Json;

namespace SVC_SendEmail
{
    public partial class Service : ServiceBase
    {
        private readonly NpgsqlConnection conn = new NpgsqlConnection(ConfigHelper.Instance.GetConnectionStringDS());
        private readonly static System.Timers.Timer timer = new System.Timers.Timer();
        private readonly int timeRepeatSecondsDefault = 5;
        private readonly int timeRepeatHoursDefault = 1;

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
                //Khởi tạo service với khoảng thời gian 1h(timeRepeatHoursDefault) 1 lần

                this.Init_Svc_Timer();
                Thread newProcess = new Thread(ListenForNotifications)
                {
                    IsBackground = true
                };
                newProcess.Start();
            }
            catch (Exception ex)
            {
                FileLogger.LogAction($"OnStart - Lỗi Khởi động : {ex}");
            }
        }

        private void Init_Svc_Timer()
        {
            //Chạy lần đầu, hệ thống sẽ tự động sau 5s
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);

            timer.Interval = TimeSpan.FromSeconds(timeRepeatSecondsDefault).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Interval = TimeSpan.FromHours(timeRepeatHoursDefault).TotalMilliseconds;
            timer.Stop();
            this.ResendEmail();
            timer.Start();
        }


        private void ResendEmail()
        {
            try
            {
                var formSearch = new FormSearch();
                var documentBLL = new DocumentBLL();
                var emailDatas = new List<EmailDataBO>();

                var listResend = documentBLL.GetRequestReSend(formSearch);

                foreach (var rq in listResend)
                {
                    formSearch.ID = rq.ID;
                    var request = documentBLL.GetRequestById(formSearch);

                    //Kiểm tra thời gian chạy luồng hiện tại có bằng = thời gian khởi tạo hay không?
                    if (request.CREATEDATTIME.TimeOfDay.Hours == DateTime.Now.TimeOfDay.Hours)
                    {
                        var lstSign = new List<DocumentSignBO>();
                        request.FILEUPLOADS.ForEach((doc) =>
                        {
                            doc.SIGN.OrderBy(x => x.SIGNINDEX).ToList().ForEach((sign) =>
                            {
                                lstSign.Add(sign);
                            });
                        });

                        var nextSign = lstSign.OrderBy(x => x.SIGNINDEX).Where(x => !x.ISSIGNED && !x.ISDECLINED).FirstOrDefault();

                        if (nextSign != null)
                        {
                            var linkViewer = BaseBLL.GenerateLinkViewer(RequestStatus.CHO_KY, nextSign.EMAILASSIGNMENT, request.ID, false, nextSign.SIGNINDEX);
                            var email = request.LISTMAILTO.Where(x => x.EMAIL == nextSign.EMAILASSIGNMENT).FirstOrDefault();
                            string Code = string.Format("{0}.{1}", request.CREATEDBYUSER, request.ID);
                            email = email ?? new ReceiverBO();
                            var subject = $"{string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.CHO_KY, " Nhắc lại - " + request.EMAILSUBJECT)}";
                            var msg = string.Format(Constants.DOCUMENT_MESSAGE_SIGN_ONE_SENT, request.FULLNAME, request.EMAIL).Replace("@", "&#64;").Replace(".", "&#46;");
                            var logo = email.ISCC ? DocumentLogo.GENERIC : DocumentLogo.REQUEST;
                            var objUser = new AccountBO() { FULLNAME = request.FULLNAME, ID = request.CREATEDBYUSER };
                            documentBLL.MakeListSendMail(request, emailDatas, linkViewer, email, subject, msg, logo, objUser, true);
                        }
                    }
                }
                AccountBLL emailBLL = new AccountBLL();
                var resultAddEmail = emailBLL.AddEmail(emailDatas);
            }
            catch (Exception objMainEx)
            {
                FileLogger.LogAction($"Exception thrown in Timer Service: {objMainEx}");
            }
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
            try
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
                        var subject = emailData.Subject.Replace("[OnSign", "OnSign").Replace("#", " ").Replace("- Từ chối]", "- Từ chối: ").Replace("- Chờ ký]", "- Chờ ký: ").Replace("- Hoàn thành]", "- Hoàn thành: ");
                        temp = temp.Replace("{emailData.Subject)}", subject);
                        temp = temp.Replace("{documentSignImageUrl}", emailData.DocumentLinkLogo);
                        temp = temp.Replace("{emailData.DocumentMessage}", emailData.DocumentMessage);
                        temp = temp.Replace("{emailData.Message}", emailData.Messages);
                        temp = temp.Replace("{emailData.MailName}", emailData.MailName);
                        temp = temp.Replace("{LinkView}", emailData.DocumentLinkViewer);
                        try
                        {
                            var strTracking = emailData.DocumentLinkViewer.Replace("?sign=", $"tracking?id={emailData.ID}&src=");

                            temp = temp.Replace("{LinkTracking}", strTracking);

                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        break;
                    case Constants.TEMPLATE_MAILTYPE_FORGOT_PASSWORD:
                        temp = temp.Replace("{emaiData.Content}", emailData.Content);
                        break;
                }
                emailData.Content = temp;
                new Thread(() =>
                {
                    var subject = emailData.Subject.Replace("[OnSign", "OnSign").Replace("#", " ").Replace("- Từ chối]", "- Từ chối: ").Replace("- Chờ ký]", "- Chờ ký: ").Replace("- Hoàn thành]", "- Hoàn thành: ").Replace("]", "");
                    var result = EmailSender.MailSender(new EmailDataBO
                    {
                        MailTo = emailData.MailTo,
                        MailName = emailData.MailName,
                        FromEmail = emailData.FromEmail,
                        FromName = emailData.FromName,
                        Subject = subject,
                        Messages = emailData.Messages,
                        Content = emailData.Content
                    });
                    if (result == true)
                    {
                        AccountBLL emailBLL = new AccountBLL();
                        emailData.IsSent = true;
                        result = emailBLL.UpdateEmail(emailData);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                FileLogger.LogAction($"Lỗi gửi email: {ex}", "");
            }
        }
    }
}