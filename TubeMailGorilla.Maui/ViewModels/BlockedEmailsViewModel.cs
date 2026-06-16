using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class BlockedEmailsViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    public ObservableCollection<Blocker> BlockedEmails { get; } = new();

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private Blocker? _selectedBlocker;
    public Blocker? SelectedBlocker { get => _selectedBlocker; set => SetProperty(ref _selectedBlocker, value); }

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }

    public BlockedEmailsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        AddCommand = new AsyncRelayCommand(AddAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    private async Task LoadAsync()
    {
        BlockedEmails.Clear();
        foreach (var b in await _unitOfWork.Blockers.GetAllAsync())
            BlockedEmails.Add(b);
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(Email)) return;
        if (await _unitOfWork.Blockers.ExistsByEmailAsync(Email)) return;
        var blocker = new Blocker { Email = Email };
        await _unitOfWork.Blockers.AddAsync(blocker);
        await _unitOfWork.CompleteAsync();
        Email = string.Empty;
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedBlocker is not null)
        {
            _unitOfWork.Blockers.Delete(SelectedBlocker);
        }
        else
        {
            var all = await _unitOfWork.Blockers.GetAllAsync();
            foreach (var b in all) _unitOfWork.Blockers.Delete(b);
        }
        await _unitOfWork.CompleteAsync();
        await LoadAsync();
    }
}
