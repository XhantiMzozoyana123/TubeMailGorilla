namespace TubeMailGorillaDomain.Entities
{
    public class Icebreaker : BaseEntity
    {
        public int EmailerId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
