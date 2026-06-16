using System.Net.Mail;
using System.Text.RegularExpressions;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using TubeMailGorillaApplication.Dtos;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Interfaces;
using TubeMailGorillaInfrastructure.Data;

namespace TubeMailGorillaInfrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> SendEmailAsync(MessengerDto message)
        {
            try
            {
                var blockers = await _unitOfWork.Blockers.GetByEmailAsync(message.EmailTo);
                var credientals = (await _unitOfWork.Credientals.GetAllAsync()).FirstOrDefault();

                if (blockers == null && credientals != null)
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(credientals.SmtpHost);

                    mail.From = new MailAddress(credientals.Email);
                    mail.To.Add(message.EmailTo);
                    mail.Subject = message.Subject;
                    mail.Body = message.Body;
                    mail.IsBodyHtml = message.Html;

                    SmtpServer.Port = credientals.SmtpPort;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(credientals.Email, credientals.Password);
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);

                    // Update emailer as checked
                    var emailers = await _unitOfWork.Emailers.GetAllAsync();
                    var emailerToUpdate = emailers.Where(x => x.Email == message.EmailTo).ToList();
                    foreach (var item in emailerToUpdate)
                    {
                        item.Checked = true;
                    }
                    await _unitOfWork.CompleteAsync();

                    return true;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                if (addr.Address != email) return false;

                string domain = email.Split('@')[1];
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(domain, QueryType.MX);
                var mxRecords = result.Answers.MxRecords().Count();
                return mxRecords > 0;
            }
            catch
            {
                return false;
            }
        }

        public string ExtractEmails(string text)
        {
            try
            {
                Regex reg = new Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}", RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(text);
                var results = matches.Select(m => m.Value).Distinct().ToList();
                return results.FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string ExtractPhoneNumbers(string text)
        {
            try
            {
                Regex reg = new Regex(@"\+?\d[\d -]{8,}\d", RegexOptions.IgnoreCase);
                MatchCollection matches = reg.Matches(text);
                var results = matches.Select(m => m.Value).Distinct().ToList();
                return results.FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string EmailConditioner(string emailTo, bool testMode, string testEmail)
        {
            return testMode ? testEmail : emailTo;
        }
    }
}

