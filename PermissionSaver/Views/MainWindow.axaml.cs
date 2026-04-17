using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PermissionSaver.ViewModels;

namespace PermissionSaver.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SerializeSourceBrowse_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Выберите папку для сериализации",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var path = folders[0].Path.LocalPath;
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SerializeSourcePath = path;
            }
        }
    }

    private async void SerializeDestBrowse_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить JSON файл",
            FileTypeChoices = new[]
            {
                    new FilePickerFileType("JSON files")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" }
                    }
                },
            DefaultExtension = "json",
            SuggestedFileName = "backup"
        });

        if (file != null)
        {
            var path = file.Path.LocalPath;
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SerializeDestPath = path;
            }
        }
    }

    private async void DeserializeSourceBrowse_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите JSON файл",
            FileTypeFilter = new[]
            {
                    new FilePickerFileType("JSON files")
                    {
                        Patterns = new[] { "*.json" },
                        MimeTypes = new[] { "application/json" }
                    }
                },
            AllowMultiple = false
        });

        if (files.Count > 0)
        {
            var path = files[0].Path.LocalPath;
            if (DataContext is MainWindowViewModel vm)
            {
                vm.DeserializeSourcePath = path;
            }
        }
    }

    private async void DeserializeDestBrowse_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Выберите папку для восстановления",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var path = folders[0].Path.LocalPath;
            if (DataContext is MainWindowViewModel vm)
            {
                vm.DeserializeDestPath = path;
            }
        }
    }

    private static TopLevel? GetTopLevel(Window window)
    {
        return TopLevel.GetTopLevel(window);
    }
}