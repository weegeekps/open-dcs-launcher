using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using OpenDCSLauncher.Models;
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

    #region Path Properties
    public string StableBinPath
    {
        get => $"{_settingsService.Settings?.GetBranch("Stable")?.BinPath}\\DCS.exe";
        set
        {
            if (value != string.Empty)
            {
                // We actually save the directory path, not the bin path.
                FileInfo fi = new FileInfo(value);

                // Not handling the exception here is intentional. There's something badly wrong if the
                // directory info is null, and we can't continue from here.
                string dirPath = fi.Directory?.Parent?.ToString()!;

                _settingsService.Settings?.CreateOrUpdateBranch("Stable", dirPath);
            }
            else
            {
                _settingsService.Settings?.RemoveBranch("Stable");
            }

            OnPropertyChanged();
        }
    }

    public string BetaBinPath
    {
        get => $"{_settingsService.Settings?.GetBranch("Beta")?.BinPath}\\DCS.exe";
        set
        {
            if (value != string.Empty)
            {
                // We actually save the directory path, not the bin path.
                FileInfo fi = new FileInfo(value);

                // Not handling the exception here is intentional. There's something badly wrong if the
                // directory info is null, and we can't continue from here.
                string dirPath = fi.Directory?.Parent?.ToString()!;

                _settingsService.Settings?.CreateOrUpdateBranch("Beta", dirPath);
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
            _stableDirectoryValidationErrorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StableDirectoryValidationErrorMessageVisibility));
        }
    }

    public string BetaDirectoryValidationErrorMessage
    {
        get => _betaDirectoryValidationErrorMessage;
        set
        {
            _betaDirectoryValidationErrorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BetaDirectoryValidationErrorMessageVisibility));
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
            if (!string.IsNullOrWhiteSpace(_stableDirectoryValidationErrorMessage) ||
                !string.IsNullOrWhiteSpace(_betaDirectoryValidationErrorMessage))
                return false;

            if (string.IsNullOrWhiteSpace(StableBinPath) &&
                string.IsNullOrWhiteSpace(BetaBinPath)) return false;

            return true;
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

        StableBinPath = file?.Path ?? string.Empty;

        OnPropertyChanged(nameof(SaveAndCloseButtonIsEnabled));
    }

    private async void BrowseForBetaDirectoryAction(Window? window)
    {
        if (window == null) throw new ArgumentNullException(nameof(window));

        var file = await CreateDcsFileOpenPicker(window).PickSingleFileAsync();
        var fi = file != null ? new FileInfo(file.Path) : null;

        var errorMessage = PerformValidation(fi);
        BetaDirectoryValidationErrorMessage = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : string.Empty;

        BetaBinPath = file?.Path ?? string.Empty;

        OnPropertyChanged(nameof(SaveAndCloseButtonIsEnabled));
    }

    private void ClearStableDirectoryAction()
    {
        StableBinPath = string.Empty;
        StableDirectoryValidationErrorMessage = string.Empty;

        OnPropertyChanged(nameof(SaveAndCloseButtonIsEnabled));
    }

    private void ClearBetaDirectoryAction()
    {
        BetaBinPath = string.Empty;
        BetaDirectoryValidationErrorMessage = string.Empty;

        OnPropertyChanged(nameof(SaveAndCloseButtonIsEnabled));
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