using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MimeKit;

namespace CloudService_API.Data
{
    public static class Auxiliary
    {
        public static string GenerateHashPassword(string password, string sSourceData)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = Encoding.ASCII.GetBytes(sSourceData);
            
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        public static string GetExtension(string path)
        {
            var split = path.Split('.');
            return $".{split.Last()}";
        }

        public static async Task SendEmailAsync(string email, string subject, string message, MailSettings mailSettings)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("CloudService", mailSettings.UserName));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            var client = await mailSettings.GetSmtpClient();
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
