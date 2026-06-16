namespace TubeMailGorillaDomain.Entities
{
    public class Inboxers : BaseEntity
    {
        public int MessageId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
