namespace TubeMailGorillaApplication.Interfaces
{
    public interface IAIService
    {
        Task<string> GenerateTextAsync(string prompt);
    }
}
