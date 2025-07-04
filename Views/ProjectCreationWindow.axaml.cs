using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using HereticalEngineEditor.Models;
using HereticalEngineEditor.ViewModels;

namespace HereticalEngineEditor.Views;

public partial class ProjectCreationWindow : Window
{
    public ProjectCreationWindow()
    {
        InitializeComponent();
        ProjectCreationViewModel vm = new ProjectCreationViewModel();
        DataContext = vm;

        vm.RequestWelcome += () =>
        {
            WelcomeWindow createWindow = new WelcomeWindow();
            createWindow.Position = new PixelPoint(Position.X - 150, Position.Y - 100);
            createWindow.Show();
            this.Close();
        };

        vm.RequestCreateProject += (ProjectData projectData) =>
        {
            MainEditorWindow createWindow = new MainEditorWindow(projectData);
            createWindow.Position = new PixelPoint(Position.X, Position.Y);
            createWindow.Show();
            this.Close();
        };
    }

    private async void OnChooseFolderClick(object sender, RoutedEventArgs e)
    {
        var storageProvider = this.StorageProvider;

        if (storageProvider.CanPickFolder)
        {
            var folder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Выберите папку для проекта",
                AllowMultiple = false
            });

            if (folder != null && folder.Count > 0)
            {
                string? selectedPath = folder[0].TryGetLocalPath();

                if (selectedPath != null)
                {
                    if (DataContext is ProjectCreationViewModel vm)
                    {
                        vm.ProjectPath = selectedPath;
                    }
                }
            }
        }
    }
}