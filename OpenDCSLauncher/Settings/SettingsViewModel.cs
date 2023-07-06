using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using OpenDCSLauncher.Services;

namespace OpenDCSLauncher.Settings;

public interface ISettingsViewModel : INotifyPropertyChanged
{
}

public class SettingsViewModel : ISettingsViewModel
{
    private readonly IStringLocalizer<SettingsViewModel> _localization;
    private readonly ISettingsService _settingsService;
    private readonly RelayCommand<Window> _browseForStableDirectoryCommand;
    private readonly RelayCommand<Window> _browseForBetaDirectoryCommand;
    private readonly RelayCommand _clearStableDirectoryCommand;
    private readonly RelayCommand _clearBetaDirectoryCommand;
    private readonly RelayCommand<Window> _saveAndCloseCommand;
    private readonly RelayCommand<Window> _closeCommand;
    private string _stableDirectoryValidationErrorMessage = string.Empty;
    private string _betaDirectoryValidationErrorMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Localization Properties
    public string BetaPathHeader => _localization["BetaPathHeader"].ToString();
    public LocalizedString ClearButtonText => _localization["ClearButtonText"];
    public LocalizedString CloseButtonText => _localization["CloseButtonText"];
    public LocalizedString FilePickerButtonText => _localization["FilePickerButtonText"];
    public LocalizedString PlaceholderText => _localization["PlaceholderText"];
    public LocalizedString SaveAndCloseButtonText => _localization["SaveAndCloseButtonText"];
    public LocalizedString SettingsWindowHeader => _localization["SettingsWindowHeader"];
    public string StablePathHeader => _localization["StablePathHeader"].ToString();
    #endregion

    #region Command Properties
    public ICommand BrowseForStableDirectoryCommand => _browseForStableDirectoryCommand;
    public ICommand BrowseForBetaDirectoryCommand => _browseForBetaDirectoryCommand;
    public ICommand ClearStableDirectoryCommand => _clearStableDirectoryCommand;
    public ICommand ClearBetaDirectoryCommand => _clearBetaDirectoryCommand;
    public ICommand SaveAndCloseCommand => _saveAndCloseCommand;
    public ICommand CloseCommand => _closeCommand;
    #endregion

    #region Directory Path Properties
    public string StableDirectoryPath
    {
        get => _settingsService.Settings?.GetBranch("Stable")?.DirectoryPath ?? string.Empty;
        set
        {
            if (value != string.Empty)
            {
                _settingsService.Settings?.CreateOrUpdateBranch("Stable", value);
            }
            else
            {
                _settingsService.Settings?.RemoveBranch("Stable");
            }

            OnPropertyChanged();
        }
    }

    public string BetaDirectoryPath
    {
        get => _settingsService.Settings?.GetBranch("Beta")?.DirectoryPath ?? string.Empty;
        set
        {
            if (value != string.Empty)
            {
                _settingsService.Settings?.CreateOrUpdateBranch("Beta", value);
            }
            else
            {
                _settingsService.Settings?.RemoveBranch("Beta");
            }

            OnPropertyChanged();
        }
    }
    #endregion

    #region Validation Properties
    public string StableDirectoryValidationErrorMessage
    {
        get => _stableDirectoryValidationErrorMessage;
        set
        {
            if (Equals(value, _stableDirectoryValidationErrorMessage)) return;
            _stableDirectoryValidationErrorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StableDirectoryValidationErrorMessageVisibility));
            OnPropertyChanged(nameof(SaveAndCloseButtonIsEnabled));
        }
    }

    public string BetaDirectoryValidationErrorMessage
    {
        get => _betaDirectoryValidationErrorMessage;
        set
        {
            if (Equals(value, _betaDirectoryValidationErrorMessage)) return;
            _betaDirectoryValidationErrorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BetaDirectoryValidationErrorMessageVisibility));
            OnPropertyChanged(nameof(SaveAndCloseButtonIsEnabled));
        }
    }

    public Visibility StableDirectoryValidationErrorMessageVisibility
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_stableDirectoryValidationErrorMessage)) return Visibility.Collapsed;

            return Visibility.Visible;
        }
    }

    public Visibility BetaDirectoryValidationErrorMessageVisibility
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_betaDirectoryValidationErrorMessage)) return Visibility.Collapsed;

            return Visibility.Visible;
        }
    }

    public bool SaveAndCloseButtonIsEnabled
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_stableDirectoryValidationErrorMessage) &&
                string.IsNullOrWhiteSpace(_betaDirectoryValidationErrorMessage)) return true;

            return false;
        }
    }
    #endregion

    public SettingsViewModel(IStringLocalizer<SettingsViewModel> localization, ISettingsService settingsService)
    {
        _localization = localization;
        _settingsService = settingsService;
        _browseForStableDirectoryCommand = new RelayCommand<Window>(BrowseForStableDirectoryAction);
        _browseForBetaDirectoryCommand = new RelayCommand<Window>(BrowseForBetaDirectoryAction);
        _clearStableDirectoryCommand = new RelayCommand(ClearStableDirectoryAction);
        _clearBetaDirectoryCommand = new RelayCommand(ClearBetaDirectoryAction);
        _saveAndCloseCommand = new RelayCommand<Window>(SaveAndCloseAction);
        _closeCommand = new RelayCommand<Window>(CloseAction);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region File Picker
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

    private string PerformValidation(FileInfo? fi)
    {
        if (fi is not { Name: "DCS.exe" })
        {
            return _localization["Error_PickDcsExe"];
        }

        var binDir = fi.Directory;
        if (binDir == null)
        {
            // TODO: If this actually happens this would be bad.
            return _localization["Error_InvalidDirectory"];
        }

        var updaterFi = new FileInfo($"{binDir}\\DCS_updater.exe");
        if (!updaterFi.Exists)
        {
            return _localization["Error_InvalidInstallationUpdaterMissing"];
        }

        return string.Empty;
    }
    #endregion

    #region Actions
    private async void BrowseForStableDirectoryAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        var file = await CreateDcsFileOpenPicker(window).PickSingleFileAsync();
        var fi = file != null ? new FileInfo(file.Path) : null;

        var errorMessage = PerformValidation(fi);
        StableDirectoryValidationErrorMessage = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : string.Empty;

        StableDirectoryPath = file?.Path ?? string.Empty;
    }

    private async void BrowseForBetaDirectoryAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        var file = await CreateDcsFileOpenPicker(window).PickSingleFileAsync();
        var fi = file != null ? new FileInfo(file.Path) : null;

        var errorMessage = PerformValidation(fi);
        BetaDirectoryValidationErrorMessage = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : string.Empty;

        BetaDirectoryPath = file?.Path ?? string.Empty;
    }

    private void ClearStableDirectoryAction()
    {
        StableDirectoryPath = string.Empty;
        StableDirectoryValidationErrorMessage = string.Empty;
    }

    private void ClearBetaDirectoryAction()
    {
        BetaDirectoryPath = string.Empty;
        BetaDirectoryValidationErrorMessage = string.Empty;
    }

    private void SaveAndCloseAction(Window? window)
    {
        _settingsService.Save();

        CloseAction(window);
    }

    private void CloseAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        window.Close();
    }
    #endregion
}