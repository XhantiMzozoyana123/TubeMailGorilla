namespace TubeMailGorillaApplication.Interfaces
{
    public interface IAIExtractorService
    {
        Task<string> GetCompanyAsync(string description, string subtitles);
        Task<string> GetFirstNameAsync(string description, string subtitles);
        Task<string> GetLastNameAsync(string description, string subtitles);
        Task<string> GetJobTitleAsync(string description, string subtitles);
        Task<string> GetLocationAsync(string description, string subtitles);
        Task<string> GetIndustryAsync(string description, string subtitles);
        Task ExtractAllAsync(TubeMailGorillaDomain.Entities.Emailer emailer);
    }
}