using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PluginBase;

namespace HereticalEngineEditor.Models.PluginTools;

[PluginData("Редактор директорий", "Встроенный плагин для управления директориями", "icons/dir.png", "directory", "Папка", "")]
public class DirectoryPlugin : PluginBaseClass, IPluginEditorUI
{
    public async Task ShowCreationDialogAsync(Window parentWindow, string path, Action<object> onCreated)
    {
        var nameBox = new TextBox { Watermark = "Имя папки" };

        var dialog = new Window
        {
            Title = "Создание папки",
            Width = 400,
            Height = 160,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SystemDecorations = SystemDecorations.BorderOnly
        };

        var createButton = new Button { Content = "Создать", IsDefault = true };
        var backButton = new Button { Content = "Назад", IsCancel = true };

        var errorText = new TextBlock
        {
            Foreground = Brushes.Red,
            IsVisible = false,
            Text = "Недопустимое имя. Используйте только буквы,\nцифры, пробелы, и нижнее подчёркивание."
        };

        createButton.Click += (_, _) =>
        {
            string folderName = nameBox.Text;

            if (IsValidFolderName(folderName))
            {
                OnCreate(path, folderName);
                dialog.Close();
            }
            else
            {
                errorText.IsVisible = true;
            }
        };

        backButton.Click += (_, _) => dialog.Close();

        dialog.Content = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(10),
            Children =
        {
            new TextBlock { Text = "Введите имя новой папки:" },
            nameBox,
            errorText,
            new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Children = {backButton, createButton }
            }
        }
        };

        await dialog.ShowDialog(parentWindow);

        if (nameBox.Text is null) nameBox.Text = "";
        onCreated.Invoke(Path.Combine(path, nameBox.Text));
    }

    private bool IsValidFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return name.All(c =>
            char.IsLetterOrDigit(c) || c == '_' ||
            (c >= 'А' && c <= 'я') || c == 'ё' || c == 'Ё' || c == ' ');
    }

    public Control GetEditorUI(string path, Action<object> onSave)
    {
        throw new NotImplementedException();
    }

    public override void OnCompile(string path, object parameters)
    {
        throw new NotImplementedException();
    }

    public override void OnCreate(string path, object parameters)
    {
        string folderName = parameters as string;

        if (string.IsNullOrWhiteSpace(folderName))
            return;

        string fullPath = Path.Combine(path, folderName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            //Console.WriteLine($"[DirectoryPlugin] Папка создана: {fullPath}");
        }
    }

    public override void OnDelete(string path, object parameters)
    {
        throw new NotImplementedException();
    }

    public override void OnMove(string oldPath, string newPath, object parameters)
    {
        throw new NotImplementedException();
    }

    public override void OnOpen(string path, object parameters)
    {
        throw new NotImplementedException();
    }

    public override void OnRename(string oldPath, string newPath, object parameters)
    {
        throw new NotImplementedException();
    }

    public override void OnSave(string path, object parameters)
    {
        throw new NotImplementedException();
    }
}
