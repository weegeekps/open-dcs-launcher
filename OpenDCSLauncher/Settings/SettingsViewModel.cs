using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;

namespace OpenDCSLauncher.Settings;

public interface ISettingsViewModel : IDisposable, INotifyPropertyChanged { }

class SettingsViewModel : ISettingsViewModel
{
    private readonly RelayCommand _browseForStableDirectoryCommand;
    private readonly RelayCommand _browseForBetaDirectoryCommand;
    private readonly RelayCommand _saveAndCloseCommand;
    private readonly RelayCommand _closeCommand;
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
            if (Equals(value, _stableDirectoryInfo?.ToString() ?? string.Empty)) return;
            _stableDirectoryInfo = new DirectoryInfo(value);
            OnPropertyChanged();
        }
    }

    public string BetaDirectoryInfo
    {
        get => _betaDirectoryInfo?.ToString() ?? string.Empty;
        set
        {
            if (!Equals(value, _betaDirectoryInfo?.ToString() ?? string.Empty)) return;
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
        _browseForStableDirectoryCommand = new RelayCommand(BrowseForStableDirectoryAction);
        _browseForBetaDirectoryCommand = new RelayCommand(BrowseForBetaDirectoryAction);
        _saveAndCloseCommand = new RelayCommand(SaveAndCloseAction);
        _closeCommand = new RelayCommand(CloseAction);
    }

    public void Dispose() { }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private static FileOpenPicker CreateDcsFileOpenPicker()
    {
        return new FileOpenPicker()
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            FileTypeFilter = { "DCS.exe" }
        };
    }

    private async void BrowseForStableDirectoryAction()
    {
        var file = await CreateDcsFileOpenPicker().PickSingleFileAsync();
        
        if (file == null)
        {
            ErrorMessage = "You must pick a DCS.exe file to continue.";
            return;
        }

        // TODO: Do more validation.

        StableDirectoryInfo = file.Path;
    }

    private async void BrowseForBetaDirectoryAction()
    {
        var file = await CreateDcsFileOpenPicker().PickSingleFileAsync();

        if (file == null)
        {
            ErrorMessage = "You must pick a DCS.exe file to continue.";
            return;
        }

        // TODO: Do more validation.

        BetaDirectoryInfo = file.Path;
    }

    private void SaveAndCloseAction()
    {
        throw new NotImplementedException();
    }

    private void CloseAction()
    {
        // TODO: Reimplement this for WinUI.

        throw new NotImplementedException();

        // Window? window = maybeWindow as Window;
        //
        // if (window != null) throw new ArgumentNullException(nameof(window));
        //
        // window.Close();
    }
}