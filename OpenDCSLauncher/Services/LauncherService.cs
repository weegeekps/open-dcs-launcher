using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenDCSLauncher.Models;

namespace OpenDCSLauncher.Services;

public interface ILauncherService
{
    public string LastErrorMessage { get; }
    public bool LaunchDcs(LauncherSelection selection);
    public bool LaunchUpdater(LauncherSelection selection);
}

public class LauncherService : ILauncherService
{
    public string LastErrorMessage { get; private set; }

    public LauncherService()
    {
        LastErrorMessage = string.Empty;
    }

    public bool LaunchDcs(LauncherSelection selection)
    {
        if (selection.BranchInfo.DirectoryPath == null)
        {
            return false;
        }

        var binPath = GetBinPathBasedOnMultiThreading(selection);
        var psi = CreateProcessStartInfo(binPath, "DCS.exe");

        return StartProcess(psi);
    }

    public bool LaunchUpdater(LauncherSelection selection)
    {
        if (selection.BranchInfo.DirectoryPath == null)
        {
            // TODO: We need to do some error handling here.
            return false;
        }

        var binPath = new DirectoryInfo(selection.BranchInfo.BinPath);
        var psi = CreateProcessStartInfo(binPath, "DCS_updater.exe", new List<string> { "update" });

        return StartProcess(psi);
    }

    private ProcessStartInfo CreateProcessStartInfo(DirectoryInfo binPath, string executable) =>
        CreateProcessStartInfo(binPath, executable, new List<string>());

    private ProcessStartInfo CreateProcessStartInfo(DirectoryInfo binPath, string executable, List<string> args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = $"{binPath}\\{executable}",
            UseShellExecute = true,
            WorkingDirectory = $"{binPath}"
        };

        if (args.Count > 0)
        {
            foreach (var item in args)
            {
                psi.ArgumentList.Add(item);
            }
        }

        return psi;
    }

    private DirectoryInfo GetBinPathBasedOnMultiThreading(LauncherSelection selection) =>
        new(selection.UseMultiThreading
            ? selection.BranchInfo.BinMtPath
            : selection.BranchInfo.BinPath);

    private bool StartProcess(ProcessStartInfo psi)
    {
        try
        {
            Process.Start(psi);
        }
        catch (Exception e)
        {
            // We are catching the exceptions here because we want to gracefully fail.
            // TODO: Log this to a log file.
            LastErrorMessage = $"{e.GetType()}: {e.Message}";
            return false;
        }

        return true;
    }
}