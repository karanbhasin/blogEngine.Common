using System;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Messaging;

namespace Foundation.Common.Util {
    public class EmailUtil {
        private static AppSettingsReader appSettings = new AppSettingsReader();

        public static void SendEmail(string from, string to, string subject, string body, bool isBodyHtml) {
            // set mail attributes
            //System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.smtp.Server"));
            //client.Port = Convert.ToInt32(Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.smtp.Server.Port"));
            //client.Credentials = new System.Net.NetworkCredential() { UserName = ConfigUtil.GetAppSetting("Global.smtp.Server.User"), Password = ConfigUtil.GetAppSetting("Global.smtp.Server.Password") };
            System.Net.Mail.SmtpClient client = Init();
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage(from, to);

            // Add headers.
            //mailMessage.Headers["Return-Path"] = (string)(appSettings.GetValue("Integration.Email.ReturnPath", typeof(string)));
            //mailMessage.Headers["X-RCPT-TO"] = mailMessage.From.Address;
            //mailMessage.Headers["X-Complaints-To"] = (string)(appSettings.GetValue("Integration.Email.Abuse", typeof(string)));
            //mailMessage.Headers["X-Mailer"] = (string)(appSettings.GetValue("Integration.Email.Mailer", typeof(string)));

            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = isBodyHtml;

            mailMessage.Body = body;

            // send the mail
            client.Send(mailMessage);
        }

        public static void SendEmail(string from, string to, string subject, string body) {
            SendEmail(from, to, subject, body, true);
        }


        public static void SendEmail(string from, string to, string cc, string subject, string body) {
            // set mail attributes
            //System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.smtp.Server"));
            //client.Credentials = new System.Net.NetworkCredential() { UserName = ConfigUtil.GetAppSetting("Global.smtp.Server.User"), Password = ConfigUtil.GetAppSetting("Global.smtp.Server.Password") };
            System.Net.Mail.SmtpClient client = Init();
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage(from, to);

            // Add headers.
            //mailMessage.Headers["Return-Path"] = (string)(appSettings.GetValue("Integration.Email.ReturnPath", typeof(string)));
            mailMessage.Headers["Sender"] = mailMessage.From.Address;
            mailMessage.Headers["X-Sender"] = mailMessage.From.Address;
            mailMessage.Headers["X-RCPT-TO"] = mailMessage.From.Address;
            //mailMessage.Headers["X-Complaints-To"] = (string)(appSettings.GetValue("Integration.Email.Abuse", typeof(string)));
            //mailMessage.Headers["X-Mailer"] = (string)(appSettings.GetValue("Integration.Email.Mailer", typeof(string)));

            if (cc != null && cc != string.Empty) {
                mailMessage.CC.Add(cc);
            }

            mailMessage.Subject = subject;

            mailMessage.IsBodyHtml = true;

            mailMessage.Body = body;

            // send the mail
            client.Send(mailMessage);
        }

        private static System.Net.Mail.SmtpClient Init() {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.smtp.Server"));
            client.Port = Convert.ToInt32(Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.smtp.Server.Port"));
            client.EnableSsl = Convert.ToBoolean(Foundation.Common.Util.ConfigUtil.GetAppSetting("Global.smtp.Server.UseSSL"));

            if (ConfigUtil.HasAppSetting("Global.smtp.Server.User") && !string.IsNullOrEmpty(ConfigUtil.GetAppSetting("Global.smtp.Server.User"))) {
                client.Credentials = new System.Net.NetworkCredential() { UserName = ConfigUtil.GetAppSetting("Global.smtp.Server.User"), Password = ConfigUtil.GetAppSetting("Global.smtp.Server.Password") };
            } else {
                client.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
            }
            return client;
        }
    }
}
