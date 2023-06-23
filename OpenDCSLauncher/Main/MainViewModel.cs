using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace OpenDCSLauncher;

public interface IMainViewModel : IDisposable, INotifyPropertyChanged { }

public class MainViewModel : IMainViewModel
{
    private readonly RelayCommand _launchCommand;
    private readonly RelayCommand _updateCommand;
    private readonly RelayCommand _manageCommand;
    private readonly RelayCommand _settingsCommand;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand LaunchCommand => _launchCommand;
    public ICommand UpdateCommand => _updateCommand;
    public ICommand ManageCommand => _manageCommand;
    public ICommand SettingsCommand => _settingsCommand;
    
    public MainViewModel()
    {
        _launchCommand = new RelayCommand(LaunchAction);
        _updateCommand = new RelayCommand(UpdateAction);
        _manageCommand = new RelayCommand(ManageAction);
        _settingsCommand = new RelayCommand(SettingsAction);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Action Handlers
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
        throw new NotImplementedException();
    }
    #endregion
}