using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using OpenDCSLauncher.Services;

namespace OpenDCSLauncher;

public interface IMainViewModel : IDisposable, INotifyPropertyChanged
{
}

public class MainViewModel : IMainViewModel
{
    private readonly IStringLocalizer<MainViewModel> _localization;
    private readonly RelayCommand _launchCommand;
    private readonly RelayCommand _updateCommand;
    private readonly RelayCommand _manageCommand;
    private readonly RelayCommand _settingsCommand;
    private readonly IWindowService _windowService;

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Localization Properties
    public LocalizedString FreeAndOpenSourceText => _localization["FreeAndOpenSourceText"];
    public LocalizedString GitHubLinkText => _localization["GitHubLinkText"];
    public LocalizedString LaunchButtonText => _localization["LaunchButtonText"];
    public LocalizedString UpdateButtonText => _localization["UpdateButtonText"];
    public LocalizedString ManageModulesButtonText => _localization["ManageModulesButtonText"];
    public LocalizedString SettingsButtonText => _localization["SettingsButtonText"];
    #endregion

    #region Command Properties
    public ICommand LaunchCommand => _launchCommand;
    public ICommand UpdateCommand => _updateCommand;
    public ICommand ManageCommand => _manageCommand;
    public ICommand SettingsCommand => _settingsCommand;
    #endregion

    public MainViewModel(IStringLocalizer<MainViewModel> localization, IWindowService windowService)
    {
        _localization = localization;
        _windowService = windowService;
        _launchCommand = new RelayCommand(LaunchAction);
        _updateCommand = new RelayCommand(UpdateAction);
        _manageCommand = new RelayCommand(ManageAction);
        _settingsCommand = new RelayCommand(SettingsAction);
    }

    public void Dispose()
    {
        // Eventually will need this when I start to incorporate some Rx flows.
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Actions
    private void LaunchAction()
    {
        throw new NotImplementedException();
    }

    private void UpdateAction()
    {
        throw new NotImplementedException();
    }

    private void ManageAction()
    {
        throw new NotImplementedException();
    }

    private void SettingsAction()
    {
        _windowService.ShowSettings();
    }
    #endregion
}