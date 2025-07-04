using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using HereticalEngineEditor.Models;

namespace HereticalEngineEditor.ViewModels
{
    public partial class ProjectCreationViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string projectName = "";

        [ObservableProperty]
        private string projectPath = "";

        [ObservableProperty]
        private string errorMessage;

        private string fullPath = "";

        public ICommand CreateCommand { get; }
        public ICommand BackCommand { get; }
        public event Action RequestWelcome;
        public event Action<ProjectData> RequestCreateProject;

        public ProjectCreationViewModel()
        {
            CreateCommand = new RelayCommand(OnCreateProject);
            BackCommand = new RelayCommand(OnBack);
        }
        partial void OnProjectNameChanged(string value)
        {
            Validate();
        }

        partial void OnProjectPathChanged(string value)
        {
            Validate();
        }

        private bool Validate()
        {
            if (ProjectPath == "")
            {
                return false;
            }

            if (!IsValidProjectName(ProjectName))
            {
                ErrorMessage = "Недопустимое имя проекта";
                return false;
            }

            fullPath = Path.GetFullPath(Path.Combine(ProjectPath, ProjectName));
            if (Directory.Exists(fullPath))
            {
                ErrorMessage = "Директория с таким именем уже существует";
                return false;
            }

            ErrorMessage = "";

            return true;
        }

        private void OnCreateProject()
        {
            RequestCreateProject?.Invoke(ProjectManager.CreateProject(fullPath, ProjectName));
        }

        private void OnBack()
        {
            RequestWelcome?.Invoke();
        }

        private bool IsValidProjectName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$");
        }
    }
}
