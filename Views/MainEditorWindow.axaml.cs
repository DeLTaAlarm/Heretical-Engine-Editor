using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HereticalEngineEditor.Models;
using HereticalEngineEditor.ViewModels;
using System;
using System.Linq;

namespace HereticalEngineEditor.Views;

public partial class MainEditorWindow : Window
{
    public MainEditorWindow(string heprojPath)
    {
        InitializeComponent();
        DataContext = new MainEditorViewModel(heprojPath);
        AttachKeyDown();

    }

    public MainEditorWindow(ProjectData heproj)
    {
        InitializeComponent();
        DataContext = new MainEditorViewModel(heproj);
        AttachKeyDown();
    }
    public MainEditorWindow()
    {
        if (!Design.IsDesignMode)
            throw new InvalidOperationException("Этот конструктор предназначен только для XAML-дизайнера.");

        InitializeComponent();
        //DataContext = new MainEditorViewModel();
        AttachKeyDown();
    }

    private void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainEditorViewModel vm)
            return;

        var flyout = new MenuFlyout();

        foreach (var option in vm.CreateOptions)
        {
            var item = new MenuItem
            {
                Header = option.Name,
                Command = option.Command
            };
            flyout.Items.Add(item);
        }

        flyout.ShowAt(CreateButton);
    }

    private void AttachKeyDown()
    {
        ContentPathTextBox.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter && DataContext is MainEditorViewModel vm)
            {
                vm.ApplyPathCommand?.Execute(e);
                e.Handled = true;
            }
        };
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is MainEditorViewModel vm)
        {
            vm.SetupCreateOptions(this);
        }
    }

    private DateTime _lastClickTime;
    private FileItemViewModel _lastClickedItem;

    private void OnItemClicked(object sender, RoutedEventArgs e)
    {
        var now = DateTime.Now;
        var control = (Control)sender;
        var item = (FileItemViewModel)control.DataContext;

        if (_lastClickedItem == item && (now - _lastClickTime).TotalMilliseconds < 400)
        {
            (DataContext as MainEditorViewModel).OpenItem(item);
            _lastClickedItem = null;
            _lastClickTime = DateTime.MinValue;
        }
        else
        {
            _lastClickedItem = item;
            _lastClickTime = now;
        }
    }
}