using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using OpenDCSLauncher.Models;
using OpenDCSLauncher.Services;

namespace OpenDCSLauncher;

public interface IMainViewModel : INotifyPropertyChanged
{
}

public class MainViewModel : IMainViewModel
{
    private readonly ILauncherService _launcherService;
    private readonly IStringLocalizer<MainViewModel> _localization;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;

    private readonly RelayCommand _launchCommand;
    private readonly RelayCommand _updateCommand;
    private readonly RelayCommand _manageCommand;
    private readonly RelayCommand _repairCommand;
    private readonly RelayCommand _cleanupCommand;
    private readonly RelayCommand _settingsCommand;

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Localization Properties
    public LocalizedString AdvancedActionsHeaderText => _localization["AdvancedActionsHeaderText"];
    public LocalizedString CleanupButtonText => _localization["CleanupButtonText"];
    public LocalizedString FreeAndOpenSourceText => _localization["FreeAndOpenSourceText"];
    public LocalizedString GitHubLinkText => _localization["GitHubLinkText"];
    public LocalizedString LaunchButtonText => _localization["LaunchButtonText"];
    public LocalizedString UpdateButtonText => _localization["UpdateButtonText"];
    public LocalizedString ManageModulesButtonText => _localization["ManageModulesButtonText"];
    public LocalizedString RepairButtonText => _localization["RepairButtonText"];
    public LocalizedString SettingsButtonText => _localization["SettingsButtonText"];
    #endregion

    #region Command Properties
    public ICommand LaunchCommand => _launchCommand;
    public ICommand UpdateCommand => _updateCommand;
    public ICommand ManageCommand => _manageCommand;
    public ICommand RepairCommand => _repairCommand;
    public ICommand CleanupCommand => _cleanupCommand;
    public ICommand SettingsCommand => _settingsCommand;
    #endregion

    #region Data Properties
    public ObservableCollection<LauncherSelection> AvailableSelections { get; private set; }
    public LauncherSelection? CurrentSelection { get; set; }
    #endregion

    public MainViewModel(ILauncherService launcherService, IStringLocalizer<MainViewModel> localization,
        ISettingsService settingsService,
        IWindowService windowService)
    {
        _launcherService = launcherService;
        _localization = localization;
        _settingsService = settingsService;
        _windowService = windowService;

        _launchCommand = new RelayCommand(LaunchAction);
        _updateCommand = new RelayCommand(UpdateAction);
        _manageCommand = new RelayCommand(ManageAction);
        _repairCommand = new RelayCommand(RepairAction);
        _cleanupCommand = new RelayCommand(CleanupAction);
        _settingsCommand = new RelayCommand(SettingsAction);

        AvailableSelections = new ObservableCollection<LauncherSelection>();
        UpdateAvailableSelections();

        _settingsService.SettingsChanged += HandleSettingsChanged;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Actions
    private void LaunchAction()
    {
        if (CurrentSelection == null) return;

        if (!_launcherService.LaunchDcs(CurrentSelection))
        {
            // TODO: Show the error to the user.
            return;
        }

        Application.Current.Exit();
    }

    private void UpdateAction()
    {
        if (CurrentSelection == null) return;

        if (!_launcherService.LaunchUpdater(CurrentSelection))
        {
            // TODO: Show the error to the user.
        }
    }

    private void ManageAction()
    {
        throw new NotImplementedException();
    }

    private void RepairAction()
    {
        if (CurrentSelection == null) return;

        if (!_launcherService.LaunchRepair(CurrentSelection))
        {
            // TODO: Show the error to the user.
        }
    }

    private void CleanupAction()
    {
        if (CurrentSelection == null) return;

        if (!_launcherService.LaunchCleanup(CurrentSelection))
        {
            // TODO: Show the error to the user.
        }
    }

    private void SettingsAction()
    {
        _windowService.ShowSettings();
    }
    #endregion

    #region Event Handlers
    private void HandleSettingsChanged(object? sender, EventArgs e)
    {
        UpdateAvailableSelections();
    }
    #endregion

    private void UpdateAvailableSelections()
    {
        AvailableSelections.Clear();

        if (_settingsService.Settings == null) return;

        foreach (var branch in _settingsService.Settings.Branches)
        {
            // Multi-threading is first because that's what I want and I assume most everyone else does too.
            // TODO: Make this an enum once Tomlyn fixes enums. https://github.com/xoofx/Tomlyn/issues/55
            if (branch.Name == "Beta")
            {
                AvailableSelections.Add(new LauncherSelection(branch, true));
            }

            AvailableSelections.Add(new LauncherSelection(branch, false));
        }

        if (AvailableSelections.Count <= 0) return;

        CurrentSelection = AvailableSelections[0];
    }
}