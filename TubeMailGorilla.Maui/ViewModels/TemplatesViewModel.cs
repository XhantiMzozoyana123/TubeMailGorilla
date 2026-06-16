using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorillaMaui.ViewModels;

public class TemplatesViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    public ObservableCollection<string> Templates { get; } = new();

    public TemplatesViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LoadDataAsync()
    {
        try
        {
            Templates.Clear();
            foreach (var temp in await _unitOfWork.Templates.GetAllAsync())
                Templates.Add(temp.Name);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}