namespace TubeMailGorilla.Mvc.Services;

public partial class CampaignJob
{
    private enum SendOutcome { Sent, Failed, Skipped }

    private async Task<SendOutcome> SendOneAsync(
        Models.EmailContact contact,
        int index,
        CampaignRequest request,
        List<Models.Sender> senders,
        List<Models.MessageParameter> parameters,
        Dictionary<int, string> latestOpenerByContact,
        List<Models.MessageVariation> variations,
        bool rotateMessages,
        CancellationToken stop)
    {
        try
        {
            if (!latestOpenerByContact.TryGetValue(contact.Id, out var icebreaker))
                return SendOutcome.Skipped;

            Models.Sender senderAccount;
            if (request.AllowAccountRotation && senders.Count > 1)
            {
                senderAccount = senders[index % senders.Count];
            }
            else
            {
                senderAccount = senders[0];
                if (request.DefaultSenderId > 0)
                    senderAccount = senders.FirstOrDefault(s => s.Id == request.DefaultSenderId) ?? senders[0];
            }

            var messageSubject = request.Subject;
            var messageBody = request.Body;
            if (rotateMessages)
            {
                var variation = variations[index % variations.Count];
                messageSubject = variation.Subject;
                messageBody = variation.Body;
            }

            var subjectText = EmailService.Personalize(messageSubject, contact, parameters, icebreaker);
            var bodyText = EmailService.Personalize(messageBody, contact, parameters, icebreaker);

            var snapshots = contact.VideoSnapshot ?? new List<string>();
            if (EmailService.ContainsSnapshotRandomToken(bodyText))
                bodyText = EmailService.PersonalizeSnapshotRandom(bodyText, snapshots);
            if (EmailService.ContainsSnapshotIndexToken(bodyText))
                bodyText = EmailService.PersonalizeSnapshotIndexed(bodyText, snapshots);
            if (EmailService.ContainsSnapshotAiToken(bodyText))
                bodyText = await EmailService.PersonalizeSnapshotAiAsync(bodyText, contact, _ai, snapshots);

            var message = new MessengerDto
            {
                EmailFrom = senderAccount.EmailAddress,
                FromName = senderAccount.Name,
                EmailTo = contact.Email,
                ToName = contact.Name ?? contact.Email,
                Subject = subjectText,
                Body = EmailService.ToHtmlBody(bodyText),
                SmtpHost = senderAccount.SmtpHost,
                SmtpPort = senderAccount.SmtpPort,
                SmtpUser = senderAccount.SmtpUser,
                SmtpPassword = senderAccount.SmtpPassword
            };

            if (await _email.SendEmailAsync(message)) return SendOutcome.Sent;
            return SendOutcome.Failed;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Campaign send to {Email} failed.", contact.Email);
            return SendOutcome.Failed;
        }
        finally
        {
            try { await Task.Delay(500, stop); } catch (OperationCanceledException) { }
        }
    }
}
