using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PermissionSaver.Services;
using System;
using System.Threading.Tasks;

namespace PermissionSaver.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _serializeSourcePath = "";

    [ObservableProperty]
    private string _serializeDestPath = "";

    [ObservableProperty]
    private string _deserializeSourcePath = "";

    [ObservableProperty]
    private string _deserializeDestPath = "";

    [ObservableProperty]
    private string _logText = "Готов к работе.\n";

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private bool _serializeSourceExists = false;

    [ObservableProperty]
    private bool _deserializeSourceExists = false;

    partial void OnSerializeSourcePathChanged(string value)
    {
        SerializeSourceExists = System.IO.File.Exists(value) || System.IO.Directory.Exists(value);
    }

    partial void OnDeserializeSourcePathChanged(string value)
    {
        DeserializeSourceExists = System.IO.File.Exists(value);
    }

    [RelayCommand]
    private async Task SerializeAsync()
    {
        if (string.IsNullOrEmpty(SerializeSourcePath) || string.IsNullOrEmpty(SerializeDestPath))
        {
            LogText += "❌ Ошибка (Сериализация): Заполните все поля!\n";
            return;
        }

        IsBusy = true;
        LogText += "🔄 Сериализация...\n";

        try
        {
            await SerializationService.SerializeAsync(SerializeSourcePath, SerializeDestPath, (msg) => UpdateLog(msg));
        }
        catch (Exception ex)
        {
            UpdateLog($"❌ Ошибка: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeserializeAsync()
    {
        if (string.IsNullOrEmpty(DeserializeSourcePath) || string.IsNullOrEmpty(DeserializeDestPath))
        {
            LogText += "❌ Ошибка (Десериализация): Заполните все поля!\n";
            return;
        }

        IsBusy = true;
        LogText += "🔄 Десериализация...\n";

        try
        {
            await SerializationService.DeserializeAsync(DeserializeSourcePath, DeserializeDestPath, (msg) => UpdateLog(msg));
        }
        catch (Exception ex)
        {
            UpdateLog($"❌ Ошибка: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateLog(string msg)
    {
        Dispatcher.UIThread.Post(() => LogText += msg + "\n");
    }
}
