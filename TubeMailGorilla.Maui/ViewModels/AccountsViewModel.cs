using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class AccountsViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    public ObservableCollection<Credientals> Accounts { get; } = new();

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private string _imapHost = "imap.gmail.com";
    public string ImapHost { get => _imapHost; set => SetProperty(ref _imapHost, value); }

    private int _imapPort = 993;
    public int ImapPort { get => _imapPort; set => SetProperty(ref _imapPort, value); }

    private string _smtpHost = "smtp.gmail.com";
    public string SmtpHost { get => _smtpHost; set => SetProperty(ref _smtpHost, value); }

    private int _smtpPort = 587;
    public int SmtpPort { get => _smtpPort; set => SetProperty(ref _smtpPort, value); }

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }

    private Credientals? _selectedAccount;
    public Credientals? SelectedAccount { get => _selectedAccount; set => SetProperty(ref _selectedAccount, value); }

    public AccountsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        AddCommand = new AsyncRelayCommand(AddAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    private async Task LoadAsync()
    {
        Accounts.Clear();
        foreach (var a in await _unitOfWork.Credientals.GetAllAsync())
            Accounts.Add(a);
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(Email)) return;
        var cred = new Credientals
        {
            Email = Email,
            Password = Password,
            ImapHost = ImapHost,
            ImapPost = ImapPort,
            SmtpHost = SmtpHost,
            SmtpPort = SmtpPort,
            ImapSsl = true,
            StmpSsl = true
        };
        await _unitOfWork.Credientals.AddAsync(cred);
        await _unitOfWork.CompleteAsync();
        Email = string.Empty;
        Password = string.Empty;
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedAccount is null) return;
        _unitOfWork.Credientals.Delete(SelectedAccount);
        await _unitOfWork.CompleteAsync();
        await LoadAsync();
    }
}
