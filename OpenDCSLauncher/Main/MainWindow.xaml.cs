using WinUIEx;

namespace OpenDCSLauncher;

/// <summary>
/// Main window containing primary launcher functionality.
/// </summary>
public sealed partial class MainWindow
{
    public MainViewModel ViewModel { get; }

    public MainWindow(IMainViewModel viewModel)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        ViewModel = (viewModel as MainViewModel)!;

        Activated += (_, _) =>
        {
            this.CenterOnScreen(500, 420);
            MinWidth = 500;
            MinHeight = 420;
        };

        Closed += (_, _) => ViewModel.Dispose();
    }
}