using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace OpenDCSLauncher.Settings;

public interface ISettingsViewModel : IDisposable, INotifyPropertyChanged { }

public class SettingsViewModel : ISettingsViewModel
{
    private readonly RelayCommand<Window> _browseForStableDirectoryCommand;
    private readonly RelayCommand<Window> _browseForBetaDirectoryCommand;
    private readonly RelayCommand<Window> _saveAndCloseCommand;
    private readonly RelayCommand<Window> _closeCommand;
    private string _errorMessage = string.Empty;
    private DirectoryInfo? _stableDirectoryInfo;
    private DirectoryInfo? _betaDirectoryInfo;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand BrowseForStableDirectoryCommand => _browseForStableDirectoryCommand;
    public ICommand BrowseForBetaDirectoryCommand => _browseForBetaDirectoryCommand;
    public ICommand SaveAndCloseCommand => _saveAndCloseCommand;
    public ICommand CloseCommand => _closeCommand;

    public string StableDirectoryInfo
    {
        get => _stableDirectoryInfo?.ToString() ?? string.Empty;
        set
        {
            _stableDirectoryInfo = new DirectoryInfo(value);
            OnPropertyChanged();
        }
    }

    public string BetaDirectoryInfo
    {
        get => _betaDirectoryInfo?.ToString() ?? string.Empty;
        set
        {
            _betaDirectoryInfo = new DirectoryInfo(value);
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (Equals(value, _errorMessage)) return;
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public SettingsViewModel()
    {
        _browseForStableDirectoryCommand = new RelayCommand<Window>(BrowseForStableDirectoryAction);
        _browseForBetaDirectoryCommand = new RelayCommand<Window>(BrowseForBetaDirectoryAction);
        _saveAndCloseCommand = new RelayCommand<Window>(SaveAndCloseAction);
        _closeCommand = new RelayCommand<Window>(CloseAction);
    }

    public void Dispose() { }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private FileOpenPicker CreateDcsFileOpenPicker(Window window)
    {
        var picker = new FileOpenPicker()
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            FileTypeFilter = { ".exe" }
        };

        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, windowHandle);

        return picker;
    }

    private async void BrowseForStableDirectoryAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        var file = await CreateDcsFileOpenPicker(window).PickSingleFileAsync();
        
        if (file == null)
        {
            ErrorMessage = "You must pick a DCS.exe file to continue.";
            return;
        }

        // TODO: Do more validation.

        StableDirectoryInfo = file.Path;
    }

    private async void BrowseForBetaDirectoryAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        var file = await CreateDcsFileOpenPicker(window).PickSingleFileAsync();

        if (file == null)
        {
            ErrorMessage = "You must pick a DCS.exe file to continue.";
            return;
        }

        // TODO: Do more validation.

        BetaDirectoryInfo = file.Path;
    }

    private void SaveAndCloseAction(Window? window)
    {
        throw new NotImplementedException();
    }

    private void CloseAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));
        
        window.Close();
    }
}