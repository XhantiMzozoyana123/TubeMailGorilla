namespace TubeMailGorillaApplication.Dtos
{
    public class OllamaDto
    {
        public class OllamaRequest
        {
            public string Model { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public bool Stream { get; set; }
        }

        public class OllamaResponse
        {
            public string Model { get; set; } = string.Empty;
            public string Response { get; set; } = string.Empty;
            public bool Done { get; set; }
        }
    }
}
