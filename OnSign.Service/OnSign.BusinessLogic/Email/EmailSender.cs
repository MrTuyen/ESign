using OnSign.BusinessObject.Email;
using OnSign.Common.Helpers;
using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace OnSign.BusinessLogic.Email
{
    public class EmailSender
    {
        public static bool MailSender(EmailDataBO emailData)
        {
            try
            {
                emailData.FromEmail = string.IsNullOrEmpty(emailData.FromEmail) ? ConfigHelper.UsernameEmail : emailData.FromEmail;
                emailData.FromName = string.IsNullOrEmpty(emailData.FromName) ? "OnSign" : emailData.FromName;
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.UseDefaultCredentials = true;

                    smtpClient.Credentials = new NetworkCredential(ConfigHelper.UsernameEmail, ConfigHelper.PasswordEmail);
                    var msg = new MailMessage
                    {
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8,
                        From = new MailAddress(emailData.FromEmail, emailData.FromName),
                        Subject = emailData.Subject,
                        Body = emailData.Content,
                        Priority = MailPriority.High,
                    };
                    if (emailData.FileName != null)
                    {
                        for (int i = 0; i < emailData.FileName.Count; i++)
                        {
                            Attachment attachment = new Attachment(emailData.StreamAttachment[i], emailData.FileName[i].ToString());
                            msg.Attachments.Add(attachment);
                        }
                    }
                    msg.To.Add(emailData.MailTo);
                    smtpClient.Send(msg);
                    smtpClient.Dispose();
                    return true;
                }
            }
            catch (Exception objEx)
            {
                ConfigHelper.Instance.WriteLogException("Đã xảy ra lỗi khi gửi mail", objEx, MethodBase.GetCurrentMethod().Name, null);
                return false;
                throw objEx;
            }
        }
    }
}
