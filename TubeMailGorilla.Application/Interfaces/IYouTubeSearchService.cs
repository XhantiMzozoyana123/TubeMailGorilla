using TubeMailGorillaApplication.Dtos;
using TubeMailGorillaDomain.Entities;

namespace TubeMailGorillaApplication.Interfaces
{
    public interface IYouTubeSearchService
    {
        Task MaxResultExtractionAsync(SearchDto searchDto, Action<string> onLog, Action<int> onProgress, CancellationToken cancellationToken = default);
        Task PageViewExtractionAsync(SearchDto searchDto, Action<string> onLog, Action<int> onProgress, CancellationToken cancellationToken = default);
    }
}
