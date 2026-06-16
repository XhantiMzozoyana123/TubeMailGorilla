namespace TubeMailGorillaApplication.Dtos
{
    public class MessengerDto
    {
        public int Id { get; set; }
        public string EmailFrom { get; set; } = string.Empty;
        public string EmailTo { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool Html { get; set; }
    }
}
