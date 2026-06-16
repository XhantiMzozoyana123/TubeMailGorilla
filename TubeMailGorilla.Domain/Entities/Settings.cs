namespace TubeMailGorillaDomain.Entities
{
    public class Settings : BaseEntity
    {
        public string ExtractType { get; set; } = string.Empty;
        public string YouTubeDataApiKey { get; set; } = string.Empty;
        public string GoogleAiApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
    }
}
