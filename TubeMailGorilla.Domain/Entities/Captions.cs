namespace TubeMailGorillaDomain.Entities
{
    public class Captions : BaseEntity
    {
        public int EmailerId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
