using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using HereticalEngineEditor.ViewModels;
using System.Collections.Generic;
using HereticalEngineEditor.Models;

namespace HereticalEngineEditor.Views;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
        WelcomeWindowViewModel vm = new WelcomeWindowViewModel();
        DataContext = vm;

        vm.RequestCreateProject += () =>
        {
            ProjectCreationWindow createWindow = new ProjectCreationWindow();
            createWindow.Position = new PixelPoint(Position.X + 150, Position.Y + 100);
            createWindow.Show();
            this.Close();
        };

        vm.RequestOpenProject += (ProjectData projectData) =>
        {
            if (projectData == null)
            {
                return;
            }

            var editorWindow = new MainEditorWindow(projectData);
            editorWindow.Show();
            this.Close();
        };
    }

    private async void OnOpenProjectClick(object sender, RoutedEventArgs e)
    {
        var storageProvider = this.StorageProvider;

        if (storageProvider.CanOpen)
        {
            var file = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть проект",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Heretical Engine Project File")
                {
                    Patterns = new[] { "*.heproj" }
                }
            }
            });

            if (file is { Count: > 0 })
            {
                string? selectedPath = file[0].TryGetLocalPath();

                if (selectedPath != null && DataContext is WelcomeWindowViewModel vm)
                {
                    vm.ProjectPath = selectedPath;
                    vm.OpenCommand.Execute(vm);
                }
            }
        }
    }

}