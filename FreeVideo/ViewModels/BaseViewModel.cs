using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace FreeVideo.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    public ICommand BackCommand => new Command(async () => {
        await Shell.Current.GoToAsync($"..");
    });
}
