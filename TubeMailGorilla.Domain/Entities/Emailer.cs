namespace TubeMailGorillaDomain.Entities
{
    public class Emailer : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // AI-extracted fields
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Keyword { get; set; } = string.Empty;
        public bool Checked { get; set; }
    }
}