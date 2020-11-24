using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace CloudService_API.Data
{
    public class MailSettings : IValidatableObject
    {
        private string Host { get; set; }
        private string Port { get; set; }
        private string UseSsl { get; set; }

        public string UserName { get; private set; }
        private string Password { get; set; }

        public MailSettings() {}
        public MailSettings(MailSettings settings)
        {
            Host = settings.Host;
            Port = settings.Port;
            UseSsl = settings.UseSsl;
            UserName = settings.UserName;
            Password = settings.Password;
        }

        public async Task<SmtpClient> GetSmtpClient()
        {
            var client = new SmtpClient();
            await client.ConnectAsync(Host, Convert.ToInt32(Port), Convert.ToBoolean(UseSsl));
            await client.AuthenticateAsync(UserName, Password);
            return client;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();
            if (string.IsNullOrWhiteSpace(Host))
            {
                Host = "smtp.mail.ru";
                errors.Add(new ValidationResult(
                    $"Для параметра {GetType().Name} -> Host задано значение по умолчанию '{Host}'"));
            }

            if (string.IsNullOrWhiteSpace(Port) || !int.TryParse(Port, out _))
            {
                Port = "465";
                errors.Add(new ValidationResult(
                    $"Для параметра {GetType().Name} -> Port задано значение по умолчанию '{Port}'"));
            }

            if (string.IsNullOrWhiteSpace(UseSsl) || !bool.TryParse(UseSsl, out _))
            {
                UseSsl = "true";
                errors.Add(new ValidationResult(
                    $"Для параметра {GetType().Name} -> UseSsl задано значение по умолчанию '{UseSsl}'"));
            }

            if (string.IsNullOrWhiteSpace(UserName))
            {
                UserName = "cloud_service@inbox.ru";
                errors.Add(new ValidationResult(
                    $"Для параметра {GetType().Name} -> UserName задано значение по умолчанию '{UserName}'"));
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Password = "YR&Iit1suoX1";
                errors.Add(new ValidationResult(
                    $"Для параметра {GetType().Name} -> Password задано значение по умолчанию '{Password}'"));
            }

            return errors;
        }
    }
}
