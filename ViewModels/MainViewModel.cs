using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HereticalEngineEditor.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase currentViewModel;

        public MainViewModel()
        {
            //CurrentViewModel = new WelcomeViewModel(this);
        }

        public void ShowProjectCreation()
        {
            //CurrentViewModel = new ProjectCreationViewModel(this);
        }

        public void ShowWelcome()
        {
            //CurrentViewModel = new WelcomeViewModel(this);
        }
    }
}
