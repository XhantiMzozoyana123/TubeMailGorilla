namespace TubeMailGorillaApplication.Interfaces
{
    public interface ICaptionService
    {
        Task<string> ExtractCaptionsAsync(string videoUrl);
    }
}
