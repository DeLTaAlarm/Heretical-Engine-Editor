using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using HereticalEngineEditor.Models;
using HereticalEngineEditor.Models.PluginTools;
using HereticalEngineEditor.ViewModels;
using PluginBase;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Avalonia.Layout;
using Avalonia.Media;

namespace HereticalEngineEditor.ViewModels;

public partial class MainEditorViewModel : ViewModelBase
{
    private ProjectData projectData;

    public string EditorTitle { get { return "Heretical Engine: " + projectData.ProjectName; } }
    public ICommand FileCommand { get; private set; }
    public ICommand FileNewCommand { get; private set; } 
    public ICommand FileOpenCommand { get; private set; }
    public ICommand FileSaveCommand { get; private set; }
    public ICommand EditCommand { get; private set; }
    public ICommand EditUndoCommand { get; private set; }
    public ICommand EditRedoCommand { get; private set; }
    public ICommand ProjectCommand { get; private set; } 
    public ICommand ProjectSettingsCommand { get; private set; }
    public ICommand BuildCommand { get; private set; }
    public ICommand SettingsCommand { get; private set; }
    public ICommand PluginsCommand { get; private set; }

    public ICommand CreateCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand BackCommand { get; private set; }
    public ICommand ApplyPathCommand => new ParameterizedRelayCommand<object>(ApplyBrowserPath);

    public ICommand OpenItemCommand => new ParameterizedRelayCommand<FileItemViewModel>(OpenItem);

    public ObservableCollection<CreateOptionViewModel> CreateOptions { get; } = new()
        {
            new CreateOptionViewModel("Папка", new RelayCommand(() => { })),
            new CreateOptionViewModel("Файл", new RelayCommand(() => { })),
        };
    public ObservableCollection<FileItemViewModel> FileItems { get; } = new();

    [ObservableProperty]
    private string contentPath = "Content/";
    private string lastContentPath = "Content/";


    private void ApplyBrowserPath(object parameter)
    {
        string normalized = ContentPath.Replace('\\', '/').TrimEnd('/');

        normalized += "/";

        string absPath = Path.Combine(projectData.projectPath, normalized);

        if (Directory.Exists(absPath) && normalized.StartsWith("Content/"))
        {
            lastContentPath = normalized;
            ContentPath = normalized;
        }
        else
        {
            string fallback = lastContentPath.Replace('\\', '/').TrimEnd('/') + "/";
            ContentPath = fallback;
            lastContentPath = fallback;
        }

        RefreshBrowser();
    }

    private void BrowserBack()
    {
        string normalizedPath = ContentPath.Replace('\\', '/').TrimEnd('/');

        if (normalizedPath.StartsWith("Content/") && normalizedPath.Length > "Content/".Length)
        {
            string[] parts = normalizedPath.Split('/');
            if (parts.Length > 1)
            {
                string newPath = string.Join('/', parts.Take(parts.Length - 1));
                newPath = newPath + (newPath.EndsWith("/") ? "" : "/");
                ContentPath = newPath;
                lastContentPath = newPath;
            }

            RefreshBrowser();
        }
    }

    private void RefreshBrowser()
    {
        LoadFiles(Path.Combine(projectData.projectPath, ContentPath));
    }

    public void LoadFiles(string path)
    {
        FileItems.Clear();
        if (Directory.Exists(path))
        {
            foreach (var dir in Directory.GetDirectories(path))
                FileItems.Add(new FileItemViewModel { Name = Path.GetFileName(dir), IsDirectory = true });

            foreach (var file in Directory.GetFiles(path))
                FileItems.Add(new FileItemViewModel { Name = Path.GetFileName(file), IsDirectory = false });
        }
    }
    public void SetupCreateOptions(Window parentWindow)
    {
        CreateOptions.Clear();

        foreach (var (plugin, data) in projectData.PluginManager.GetPlugins())
        {
            if (plugin is IPluginEditorUI iplugin)
            {
                string name = data.fileExtensionDisplayName ?? data.displayName;

                CreateOptions.Add(new CreateOptionViewModel(name, new RelayCommand(async () =>
                {
                    await iplugin.ShowCreationDialogAsync(parentWindow, Path.Combine(projectData.projectPath, contentPath), OnFileCreated);
                })));
            }
        }
    }

    public void OnFileCreated(object obj)
    {
        RefreshBrowser();
    }

    public void OpenItem(FileItemViewModel item)
    {
        if (item.IsDirectory)
        {
            string tPath = Path.Combine(ContentPath, item.Name).Replace('\\', '/').TrimEnd('/');
            if (Path.Exists(Path.Combine(projectData.projectPath, tPath)))
            {
                ContentPath = tPath;
                lastContentPath = ContentPath;
                RefreshBrowser();
            }
        }
        else
        {
            string tPath = Path.Combine(ContentPath, item.Name).Replace('\\', '/').TrimEnd('/');
            string fullPath = Path.Combine(projectData.projectPath, tPath);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

    }

    public async Task BuildProject(Window? parentWindow = null)
    {
        var builder = new ProjectBuilder(Path.Combine(projectData.projectPath, "Source"), projectData.ProjectName);
        string target = Path.Combine(projectData.projectPath, "Bin");


        string outputExe = "";
        if (builder.BuildProject(out outputExe, out string log, target))
        {
            var successWindow = new MessageWindow("Сборка успешна", $"Файл успешно собран:\n{target}");
            await successWindow.ShowDialogAsync(parentWindow);
        }
        else
        {
            var errorWindow = new MessageWindow("Ошибка сборки", $"Сборка не удалась:\n{log}");
            await errorWindow.ShowDialogAsync(parentWindow);
        }
    }

    public MainEditorViewModel(string heprojPath)
    {
        projectData = ProjectManager.OpenEngineProject(heprojPath);
        Initialize();
    }

    public MainEditorViewModel(ProjectData heproj)
    {
        projectData = heproj;
        Initialize();
    }

    public MainEditorViewModel()
    {
        Initialize();
    }

    private void Initialize()
    {
        FileCommand = new RelayCommand(() => {});
        FileNewCommand = new RelayCommand(() => {});
        FileOpenCommand = new RelayCommand(() => {});
        FileSaveCommand = new RelayCommand(() => {});
        EditCommand = new RelayCommand(() => {});
        EditUndoCommand = new RelayCommand(() => {});
        EditRedoCommand = new RelayCommand(() => {});
        ProjectCommand = new RelayCommand(() => {});
        ProjectSettingsCommand = new RelayCommand(() => {});
        BuildCommand = new RelayCommand(async () => await BuildProject());
        SettingsCommand = new RelayCommand(() => {});
        PluginsCommand = new RelayCommand(() => { });

        CreateCommand = new RelayCommand(() => { });
        RefreshCommand = new RelayCommand(RefreshBrowser);
        BackCommand = new RelayCommand(BrowserBack);

        RefreshBrowser();
    }
}


public class MessageWindow : Window
{
public MessageWindow(string title, string message)
{
    Title = title;
    Width = 400;
    Height = 200;
    WindowStartupLocation = WindowStartupLocation.CenterScreen;

    var textBlock = new TextBlock
    {
        Text = message,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(10),
        FontSize = 14
    };

    var okButton = new Button
    {
        Content = "OK",
        Width = 80,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 10, 0, 0)
    };
    okButton.Click += (_, _) => Close();

    var stackPanel = new StackPanel
    {
        Orientation = Orientation.Vertical,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Center
    };

    stackPanel.Children.Add(textBlock);
    stackPanel.Children.Add(okButton);

    Content = stackPanel;
}

public async Task ShowDialogAsync(Window? owner = null)
{
    if (owner != null)
        await ShowDialog(owner);
    else
        Show();
}
}