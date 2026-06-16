using TubeMailGorillaApplication.Dtos;

namespace TubeMailGorillaApplication.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(MessengerDto message);
        Task<bool> ValidateEmailAsync(string email);
        string ExtractEmails(string text);
        string ExtractPhoneNumbers(string text);
        string EmailConditioner(string emailTo, bool testMode, string testEmail);
    }
}
