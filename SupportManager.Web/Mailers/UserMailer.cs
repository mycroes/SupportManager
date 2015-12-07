using System.Net.Mail;

namespace SupportManager.Web.Mailers
{
    public class UserMailer
    {
        private readonly SmtpClient smtpClient;

        public UserMailer(SmtpClient smtpClient)
        {
            this.smtpClient = smtpClient;
        }

        public void EmailVerificationCode(string emailAddress, string userName, string verificationUrl)
        {
            var message = new MailMessage
            {
                From = new MailAddress(emailAddress, userName),
                Subject = "SupportManager emailaddress verification",
                Body = $"Dear {userName},\n\nPlease follow the link below to verify this emailaddress:\n\n"
                       + $"\t{verificationUrl}\n\nRegards,\n\nSupportManager"
            };
            message.To.Add(new MailAddress(emailAddress, userName));
            smtpClient.Send(message);
        }
    }
}