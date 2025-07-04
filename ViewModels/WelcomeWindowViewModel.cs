using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HereticalEngineEditor.Models;

namespace HereticalEngineEditor.ViewModels
{
    public partial class WelcomeWindowViewModel : ViewModelBase
    {
        public ICommand CreateCommand { get; }
        public ICommand OpenCommand { get; }
        public event Action RequestCreateProject;
        public event Action<ProjectData> RequestOpenProject;

        [ObservableProperty]
        private string projectPath;

        public WelcomeWindowViewModel()
        {
            CreateCommand = new RelayCommand(OnCreateProject);
            OpenCommand = new RelayCommand(OnOpenProject);
        }

        private void OnCreateProject()
        {
            RequestCreateProject?.Invoke();
        }

        private void OnOpenProject()
        {
            RequestOpenProject?.Invoke(ProjectManager.OpenEngineProject(ProjectPath));
        }

        
    }
}
