using System.Text.Json;
using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// Centralised access to the send-emails / extract preferences used across pages.
/// All values are persisted with MAUI Preferences.
/// </summary>
public static class SendSettings
{
    private const string AllowAccountRotationKey = "AllowAccountRotation";
    private const string AllowMessageRotationKey = "AllowMessageRotation";
    private const string GmailOnlyKey = "ExtractGmailOnly";
    private const string ValidateEmailsKey = "ExtractValidateEmails";
            private const string InboxReadCountKey = "InboxReadCount";
    private const string DefaultSenderIdKey = "DefaultSenderId";
    private const string MessageVariationsKey = "MessageVariations";

    public static bool AllowAccountRotation
    {
        get => Preferences.Default.Get(AllowAccountRotationKey, false);
        set => Preferences.Default.Set(AllowAccountRotationKey, value);
    }

        public static bool AllowMessageRotation
    {
        get => Preferences.Default.Get(AllowMessageRotationKey, false);
        set => Preferences.Default.Set(AllowMessageRotationKey, value);
    }

    /// <summary>
    /// The account to use when <see cref="AllowAccountRotation"/> is off.
    /// A value of 0 means "automatic" (the first active account).
    /// </summary>
    public static int DefaultSenderId
    {
        get => Preferences.Default.Get(DefaultSenderIdKey, 0);
        set => Preferences.Default.Set(DefaultSenderIdKey, value);
    }

    public static bool ExtractGmailOnly
    {
        get => Preferences.Default.Get(GmailOnlyKey, false);
        set => Preferences.Default.Set(GmailOnlyKey, value);
    }

    public static bool ExtractValidateEmails
    {
        get => Preferences.Default.Get(ValidateEmailsKey, true);
        set => Preferences.Default.Set(ValidateEmailsKey, value);
    }

    /// <summary>How many new inbox messages the inbox refresh should read through.</summary>
    public static int InboxReadCount
    {
        get => Math.Max(1, Preferences.Default.Get(InboxReadCountKey, 50));
        set => Preferences.Default.Set(InboxReadCountKey, Math.Max(1, value));
    }

    /// <summary>
    /// Alternate messages used when <see cref="AllowMessageRotation"/> is on.
    /// Each takes a turn across the campaign's recipients.
    /// </summary>
    public static List<MessageVariation> MessageVariations
    {
        get
        {
            try
            {
                return JsonSerializer.Deserialize<List<MessageVariation>>(
                    Preferences.Default.Get(MessageVariationsKey, "[]")) ?? new List<MessageVariation>();
            }
            catch
            {
                return new List<MessageVariation>();
            }
        }
        set => Preferences.Default.Set(MessageVariationsKey, JsonSerializer.Serialize(value));
    }

    /// <summary>Clears the local application/account state (acts as a local logout).</summary>
    public static void ClearSession()
    {
        foreach (var key in new[]
        {
            AllowAccountRotationKey, AllowMessageRotationKey,
            GmailOnlyKey, ValidateEmailsKey, InboxReadCountKey,
            DefaultSenderIdKey,
            "GeminiApiKey", "ActiveSubscription"
        })
        {
            Preferences.Default.Remove(key);
        }
    }
}

/// <summary>
/// One alternate message used in message rotation. Tokens like [name]
/// are personalized per recipient at send time.
/// </summary>
public record MessageVariation(string Subject, string Body);