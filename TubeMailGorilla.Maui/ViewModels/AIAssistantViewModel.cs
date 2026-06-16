using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TubeMailGorillaApplication.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class AIAssistantViewModel : BaseViewModel
{
    private readonly IAIService _aiService;
    private readonly IAIExtractorService _extractorService;

    private string _prompt = string.Empty;
    public string Prompt { get => _prompt; set => SetProperty(ref _prompt, value); }

    private string _response = string.Empty;
    public string Response { get => _response; set => SetProperty(ref _response, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public IAsyncRelayCommand GenerateCommand { get; }

    public AIAssistantViewModel(IAIService aiService, IAIExtractorService extractorService)
    {
        _aiService = aiService;
        _extractorService = extractorService;
        GenerateCommand = new AsyncRelayCommand(GenerateAsync);
    }

    private async Task GenerateAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Prompt)) return;
        IsBusy = true;
        try
        {
            Response = await _aiService.GenerateTextAsync(Prompt);
        }
        catch (Exception ex)
        {
            Response = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}