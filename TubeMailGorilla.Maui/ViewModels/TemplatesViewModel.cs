using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class TemplatesViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    public ObservableCollection<Templates> TemplateList { get; } = new();

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _subject = string.Empty;
    public string Subject { get => _subject; set => SetProperty(ref _subject, value); }

    private string _body = string.Empty;
    public string Body { get => _body; set => SetProperty(ref _body, value); }

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand LoadCommand { get; }

    public TemplatesViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        AddCommand = new AsyncRelayCommand(AddAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    private async Task LoadAsync()
    {
        TemplateList.Clear();
        foreach (var t in await _unitOfWork.Templates.GetAllAsync())
            TemplateList.Add(t);
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        var template = new Templates { Name = Name, Subject = Subject, Body = Body, Html = false };
        await _unitOfWork.Templates.AddAsync(template);
        await _unitOfWork.CompleteAsync();
        Name = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        var all = await _unitOfWork.Templates.GetAllAsync();
        foreach (var t in all)
            _unitOfWork.Templates.Delete(t);
        await _unitOfWork.CompleteAsync();
        await LoadAsync();
    }

}