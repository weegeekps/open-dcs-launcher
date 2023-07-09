namespace OpenDCSLauncher.Models;

public class LauncherSelection
{
    public readonly BranchInfo BranchInfo;
    public bool UseMultiThreading;

    public LauncherSelection(BranchInfo branchInfo, bool useMultiThreading)
    {
        BranchInfo = branchInfo;
        UseMultiThreading = useMultiThreading;
    }

    public override string ToString()
    {
        var displayString = BranchInfo.Name ?? "Unknown";

        if (UseMultiThreading) displayString += " (Multi-threaded)";

        return displayString;
    }
}