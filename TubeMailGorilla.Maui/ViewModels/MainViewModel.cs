using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TubeMailGorillaApplication.Dtos;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IYouTubeSearchService _youTubeSearchService;
    private readonly IEmailService _emailService;

    public ObservableCollection<string> Results { get; } = new();

    private string _keyword = string.Empty;
    public string Keyword { get => _keyword; set => SetProperty(ref _keyword, value); }

    private string _uniqueEmailsFound = "0";
    public string UniqueEmailsFound { get => _uniqueEmailsFound; set => SetProperty(ref _uniqueEmailsFound, value); }

    private string _emailsForwarded = "0";
    public string EmailsForwarded { get => _emailsForwarded; set => SetProperty(ref _emailsForwarded, value); }

    private string _emailReplies = "0";
    public string EmailReplies { get => _emailReplies; set => SetProperty(ref _emailReplies, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    private string _subject = string.Empty;
    public string Subject { get => _subject; set => SetProperty(ref _subject, value); }

    private string _body = string.Empty;
    public string Body { get => _body; set => SetProperty(ref _body, value); }

    public ObservableCollection<string> EmailAddresses { get; } = new();
    public ObservableCollection<string> Templates { get; } = new();

    private string? _selectedEmail;
    public string? SelectedEmail { get => _selectedEmail; set => SetProperty(ref _selectedEmail, value); }

    private string? _selectedTemplate;
    public string? SelectedTemplate { get => _selectedTemplate; set => SetProperty(ref _selectedTemplate, value); }

    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand SendEmailCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public MainViewModel(
        IUnitOfWork unitOfWork,
        IYouTubeSearchService youTubeSearchService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _youTubeSearchService = youTubeSearchService;
        _emailService = emailService;

        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SendEmailCommand = new AsyncRelayCommand(SendEmailAsync);
        RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var emailers = await _unitOfWork.Emailers.GetAllAsync();
            var inboxers = await _unitOfWork.Inboxers.GetAllAsync();

            UniqueEmailsFound = emailers.Where(x => !x.Checked).GroupBy(g => g.Email).Count().ToString();
            EmailsForwarded = emailers.Where(x => x.Checked).Count().ToString();
            EmailReplies = inboxers.Count().ToString();

            EmailAddresses.Clear();
            foreach (var item in await _unitOfWork.Credentials.GetAllAsync())
                EmailAddresses.Add(item.Email);

            Templates.Clear();
            foreach (var temp in await _unitOfWork.Templates.GetAllAsync())
                Templates.Add(temp.Name);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task SearchAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Keyword)) return;

        IsBusy = true;
        try
        {
            var searchDto = new SearchDto
            {
                Keywords = Keyword,
                PageViewLimit = 50,
                NumberofChannelsVideos = 10,
                NumberofViewCount = 100,
                NumberofLikeCount = 10
            };

            await _youTubeSearchService.MaxResultExtractionAsync(
                searchDto,
                msg => MainThread.BeginInvokeOnMainThread(() => Results.Add(msg)),
                val => { });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SendEmailAsync()
    {
        try
        {
            var emailers = await _unitOfWork.Emailers.GetAllAsync();
            var uncheckedEmails = emailers.Where(x => !x.Checked).ToList();

            foreach (var item in uncheckedEmails)
            {
                var message = new MessengerDto
                {
                    EmailTo = item.Email,
                    Subject = Subject.Replace("[name]", item.Author).Replace("[title]", item.Title),
                    Body = Body.Replace("[name]", item.Author).Replace("[title]", item.Title),
                    Html = true
                };

                await _emailService.SendEmailAsync(message);
            }

            await Shell.Current.DisplayAlert("Success", "Emails forwarded successfully.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}