using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SendMail.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SendMail.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 發送信件 by System.Net.Mail
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> SendMail()
        {
            try
            {
                string SenderAddress = _configuration.GetValue<string>("MailNotify:SenderAddress");
                string Password = _configuration.GetValue<string>("MailNotify:Password");
                string SenderName = _configuration.GetValue<string>("MailNotify:SenderName");
                string SmtpServer = _configuration.GetValue<string>("MailNotify:SmtpServer");
                int SmtpPort = _configuration.GetValue<int>("MailNotify:SmtpPort");
                bool EnableSsl = _configuration.GetValue<bool>("MailNotify:EnableSsl");
                bool UseDefaultCredentials = _configuration.GetValue<bool>("MailNotify:UseDefaultCredentials");

                string RecieverAddress = "duncky845@gmail.com";
                string RecieverName = "duncky845";
                string Subject = "測試發送";
                string Message = "測試發送";

                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(SenderAddress, SenderName, Encoding.UTF8);
                    message.SubjectEncoding = Encoding.UTF8;
                    message.BodyEncoding = Encoding.UTF8;
                    message.Subject = Subject;
                    message.Body = Message;
                    message.IsBodyHtml = true;
                    message.To.Add(RecieverAddress);
                    using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient())
                    {
                        client.UseDefaultCredentials = UseDefaultCredentials;
                        client.Credentials = new NetworkCredential(SenderAddress, Password);
                        client.Host = SmtpServer;
                        client.EnableSsl = EnableSsl;
                        client.Send(message);
                    }
                    return Content("Mail 發送成功");
                }
            }
            catch
            {
                return Content("Mail 發送失敗");
            }
        }


        /// <summary>
        /// 發送信件 by MailKit
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> SendMailByMailKit()
        {
            string SenderAddress = _configuration.GetValue<string>("MailNotify:SenderAddress");
            string Password = _configuration.GetValue<string>("MailNotify:Password");
            string SenderName = _configuration.GetValue<string>("MailNotify:SenderName");
            string SmtpServer = _configuration.GetValue<string>("MailNotify:SmtpServer");
            int SmtpPort = _configuration.GetValue<int>("MailNotify:SmtpPort");
            bool EnableSsl = _configuration.GetValue<bool>("MailNotify:EnableSsl");
            bool UseDefaultCredentials = _configuration.GetValue<bool>("MailNotify:UseDefaultCredentials");

            string RecieverAddress = "duncky845@gmail.com";
            string RecieverName = "duncky845";
            string Subject = "測試發送";
            string Message = "測試發送";

            #region  MailKit
            // 設定寄件人、郵件主題
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(SenderName, SenderAddress));
            mimeMessage.Subject = Subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = Message };
            // 設定收件人 (支援多位收件人，以;代表多位收件人間隔符號)
            foreach (string AddressItem in RecieverAddress.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                mimeMessage.To.Add(new MailboxAddress(Encoding.UTF8, RecieverName, AddressItem));
            };

            // 發送郵件
            using (MailKit.Net.Smtp.SmtpClient smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                // Note: since we don't have an OAuth2 token, disable 
                smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                smtp.CheckCertificateRevocation = false;
                try
                {
                    await smtp.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.Auto);
                    //SMTP server requires 驗證
                    await smtp.AuthenticateAsync(SenderAddress, Password);
                    await smtp.SendAsync(mimeMessage);
                    await smtp.DisconnectAsync(true);
                    return Content("Mail 發送成功");
                }
                catch
                {
                    return Content("Mail 發送失敗");
                }
            }
            #endregion
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}