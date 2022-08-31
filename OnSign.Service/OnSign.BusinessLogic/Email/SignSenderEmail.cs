using OnSign.BusinessObject.Email;
using OnSign.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Email
{
    public class SignSenderEmail
    {
        public void SendEmailSign(EmailDataBO emailData)
        {
            string logoDownloadApp = "https://s.onfinance.asia/Images/download_app.png";
            string logoUrl = "https://s.onfinance.asia/Images/logo-hori.png";
            string documentSignImageUrl = string.IsNullOrEmpty(emailData.DocumentLinkLogo) ? "https://s.onfinance.asia/Images/documentsign.png" : emailData.DocumentLinkLogo;
            string supportUrl = "https://support.onfinance.asia/";
            string LinkView = emailData.Link;
            new Thread(() =>
            {
                EmailSender.MailSender(new EmailDataBO
                {
                    MailTo = emailData.MailTo,
                    MailName = emailData.MailName,
                    FromEmail = emailData.FromEmail,
                    FromName = emailData.FromName,
                    Subject = emailData.Subject,
                    Content = $"<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
                    $"<head>" +
                    $"<title>Email Template</title>" +
                    $"<meta charset=\"utf-8\">" +
                    $"</head>" +
                    $"<body>" +
                    $"<div>" +
                    $"<div style=\"background-color:#eaeaea;padding:2%;font-family:Helvetica,Arial,Sans Serif;\">" +
                    $"<img style=\"display:none\"/>" +
                    $"<table role=\"presentation\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"100%\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td></td>" +
                    $"<td width=\"640\">" +
                    $"<table style=\"border-collapse:collapse;background-color:#ffffff;max-width:640px\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td style=\"padding:10px 24px\">" +
                    $"<img style=\"border:none\" width=\"116\" src=\"{logoUrl}\" alt=\"OnSign\"/>" +
                    $"</td>" +
                    $"</tr>" +
                    $"<tr>" +
                    $"<td style=\"padding:0px 24px 30px 24px\">" +
                    $"<table role=\"presentation\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"" +
                    $"width=\"100%\" align=\"center\" style=\"background-color:green;color:#ffffff\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td style=\"padding:28px 36px 36px 36px;border-radius:2px;background-color:green;color:#ffffff;font-size:16px;font-family:Helvetica,Arial,Sans Serif;width:100%;text-align:center\" align=\"center\">" +
                    $"<img width=\"75\" height=\"75\" src=\"{documentSignImageUrl}\" alt=\"\"/>" +
                    $"<table role=\"presentation\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td style=\"padding-top:24px;font-size:16px;font-family:Helvetica,Arial,Sans Serif;border:none;text-align:center;color:#ffffff\" align=\"center\">" +
                    $"{emailData.DocumentMessage}</td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"<table role=\"presentation\" border=\"0\" cellspacing=\"0\"" +
                    $"cellpadding=\"0\" width=\"100%\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td align=\"center\" style=\"padding-top:30px\">" +
                    $"<div>" +
                    $"<table cellspacing=\"0\" cellpadding=\"0\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td align=\"center\" height=\"44\"" +
                    $"style=\"font-size:15px;color:#333333;background-color:#ffc423;font-family:Helvetica,Arial,Sans Serif;font-weight:bold;text-align:center;text-decoration:none;border-radius:2px;background-color:#ffc423;display:block\">" +
                    $"<a href=\"{LinkView}\" style=\"font-size:15px;color:#333333;background-color:#ffc423;font-family:Helvetica,Arial,Sans Serif;font-weight:bold;text-align:center;text-decoration:none;border-radius:2px;background-color:#ffc423;display:inline-block\" data-saferedirecturl=\"https://www.google.com/url?q={LinkView}&source=gmail\">" +
                    $"<span style=\"padding:0px 24px;line-height:44px\"> XEM XÉT TÀI LIỆU </span>" +
                    $"</a>" +
                    $"</td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"</div>" +
                    $"</td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"</td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"</td>" +
                    $"</tr>" +
                    $"<tr>" +
                    $"<td style=\"padding:0px 24px 24px 24px;color:#000000;font-size:16px;font-family:Helvetica,Arial,Sans Serif;background-color:white\">" +
                    $"<table role=\"presentation\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">" +
                    $"<tbody>" +
                    $"<tr>" +
                    $"<td style=\"padding-bottom:20px\">" +
                    $"<div style=\"font-family:Helvetica,Arial,Sans Serif;font-weight:bold;line-height:18px;font-size:15px;color:#333333\">" +
                    $"<strong>Xin chào {emailData.MailName},</strong>" +
                    $"</div>" +
                    $"</td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"<span style=\"font-size:15px;color:#333333;font-family:Helvetica,Arial,Sans Serif;line-height:20px\">" +
                    $"{emailData.Message}" +
                    $"<br>" +
                    $"</span>" +
                    $"<br>" +
                    $"</td>" +
                    $"</tr>" +
                    $"<tr>" +
                    $"<td style=\"padding:0px 24px 12px 24px;background-color:#ffffff;font-family:Helvetica,Arial,Sans Serif;font-size:11px;color:#666666\"></td>" +
                    $"</tr>" +
                    $"<tr>" +
                    $"<td style=\"padding:30px 24px 45px 24px;background-color:#eaeaea\">" +
                    $"<p style=\"margin-bottom:1em;font-family:Helvetica,Arial,Sans Serif;font-size:13px;color:#666666;line-height:18px\">" +
                    $"<b aria-level=\"3\" role=\"heading\">Không chia sẻ email này</b>" +
                    $"<br>" +
                    $"Email này chứa liên kết bảo mật tới OnSign. Vui lòng không chia sẻ email này, liên kết với người khác." +
                    $"<br>" +
                    $"</p>" +
                    $"<p style=\"margin-bottom:1em;font-family:Helvetica,Arial,Sans Serif;font-size:13px;color:#666666;line-height:18px\">" +
                    $"<b aria-level=\"3\" role=\"heading\">Về OnSign</b>" +
                    $"<br>" +
                    $"Ký tài liệu điện tử nhanh chóng. An toàn, bảo mât, và hợp pháp. Bất kể bạn ở văn phòng, ở nhà, trên đường thậm chí ở nước ngoài -- OnSign cung cấp giải pháp chuyên nghiệp và tin cậy cho quản lý giao dịch số." +
                    $"</p>" +
                    $"<p style=\"margin-bottom:1em;font-family:Helvetica,Arial,Sans Serif;font-size:13px;color:#666666;line-height:18px\">" +
                    $"<b aria-level=\"3\" role=\"heading\">Câu hỏi về tài liệu?</b>" +
                    $"<br>" +
                    $"Nếu bạn cần chỉnh sửa tài liệu hoặc có bất kỳ câu hỏi nào về chi tiết trong tài liệu, vui lòng liên hệ người gửi bằng cách gửi email trực tiếp." +
                    $"<br>" +
                    $"<br> Nếu bạn gặp vấn đề khi ký tài liệu, vui lòng liên hệ hỗ trợ " +
                    $"<a href=\"{supportUrl}\" style=\"color:#2463d1\" target=\"_blank\" data-saferedirecturl=\"https://www.google.com/url?q={supportUrl}&source=gmail\">tại đây</a>." +
                    $"<br>" +
                    $"<br>" +
                    $"</p>" +
                    $"<p style=\"margin-bottom:1em;font-family:Helvetica,Arial,Sans Serif;font-size:13px;color:#666666;line-height:18px\">" +
                    $"<a href=\"https://s.onfinance.asia/tai-xuong\" style=\"color:#2463d1\" target=\"_blank\" data-saferedirecturl=\"https://www.google.com/url?q=https://s.onfinance.asia/tai-xuong&source=gmail\">" +
                    $"<img style=\"margin-right:7px;border:none;vertical-align:middle\" width=\"32\" height=\"32\" src=\"{logoDownloadApp}\" alt=\"\"/>Tải ứng dụng ký trên điện thoại" +
                    $"</a>" +
                    $"</p>" +
                    $"</td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"</td>" +
                    $"<td></td>" +
                    $"</tr>" +
                    $"</tbody>" +
                    $"</table>" +
                    $"</div>" +
                    $"</div>" +
                    $"</body>" +
                    $"</html>"
                });
            }).Start();

        }

    }
}
