﻿using Microsoft.UI.Xaml;
using System;
using Microsoft.Extensions.DependencyInjection;
using OpenDCSLauncher.Services;
using OpenDCSLauncher.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OpenDCSLauncher;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App
{
    public IServiceProvider ServiceProvider { get; }
    public static MainWindow MainWindow { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();

        #region .NET Services
        services.AddLogging();
        services.AddLocalization();
        #endregion

        #region View Models
        services.AddScoped<IMainViewModel, MainViewModel>();
        services.AddScoped<ISettingsViewModel, SettingsViewModel>();
        #endregion

        #region Services
        services.AddScoped<IWindowService, WindowService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        #endregion

        #region Windows
        services.AddTransient(typeof(MainWindow));
        services.AddTransient(typeof(SettingsWindow));
        #endregion

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var settingsService = ServiceProvider.GetService<ISettingsService>();
        settingsService?.Load();

        MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow.Activate();
    }
}