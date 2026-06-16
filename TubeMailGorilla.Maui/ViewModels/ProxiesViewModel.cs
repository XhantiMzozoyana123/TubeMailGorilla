using System.Collections.ObjectModel;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class ProxiesViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    public ObservableCollection<Proxies> ProxyList { get; } = new();

    private string _proxyAddress = string.Empty;
    public string ProxyAddress { get => _proxyAddress; set => SetProperty(ref _proxyAddress, value); }

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand ClearCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }

    public ProxiesViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        AddCommand = new AsyncRelayCommand(AddAsync);
        ClearCommand = new AsyncRelayCommand(ClearAsync);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    private async Task LoadAsync()
    {
        ProxyList.Clear();
        foreach (var p in await _unitOfWork.Proxies.GetAllAsync())
            ProxyList.Add(p);
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(ProxyAddress)) return;
        var proxy = new Proxies { Address = ProxyAddress };
        _unitOfWork.Proxies.Add(proxy);
        await _unitOfWork.CompleteAsync();
        await LoadAsync();
    }

    private async Task ClearAsync()
    {
        var all = await _unitOfWork.Proxies.GetAllAsync();
        foreach (var p in all) _unitOfWork.Proxies.Delete(p);
        await _unitOfWork.CompleteAsync();
        await LoadAsync();
    }
}