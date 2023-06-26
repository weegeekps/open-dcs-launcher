using System;
using Microsoft.Extensions.DependencyInjection;
using OpenDCSLauncher.Settings;

namespace OpenDCSLauncher.Services;

public interface IWindowService
{
    void ShowSettings();
}

class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;

    public WindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets an instance of the settings window from the service provider and activates it.
    /// </summary>
    public void ShowSettings()
    {
        var window = _serviceProvider.GetRequiredService<SettingsWindow>();
        window.Activate();
    }
}