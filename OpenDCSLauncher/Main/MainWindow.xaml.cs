namespace OpenDCSLauncher;

public sealed partial class MainWindow
{
    public MainViewModel ViewModel { get; private set; }

    public MainWindow(IMainViewModel viewModel)
    {
        InitializeComponent();

        Title = "Open DCS Launcher";

        ViewModel = (viewModel as MainViewModel)!;
    }
}