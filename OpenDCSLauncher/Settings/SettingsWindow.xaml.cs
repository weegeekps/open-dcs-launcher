using Microsoft.UI.Xaml.Input;
using WinUIEx;

namespace OpenDCSLauncher.Settings;

/// <summary>
/// Window for managing application settings.
/// </summary>
public sealed partial class SettingsWindow
{
    public SettingsViewModel ViewModel { get; }

    public SettingsWindow(ISettingsViewModel viewModel)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        ViewModel = (viewModel as SettingsViewModel)!;

        Activated += (_, _) => { this.CenterOnScreen(600, 400); };
    }
}