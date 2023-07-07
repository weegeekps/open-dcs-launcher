using System;
using System.IO;
using System.Threading.Tasks;
using OpenDCSLauncher.Models;
using Tomlyn;

namespace OpenDCSLauncher.Services;

public interface ISettingsService
{
    public SettingsModel? Settings { get; }
    public Task Load();
    public Task<bool> Save();
    public event EventHandler SettingsChanged;
}

public class SettingsService : ISettingsService
{
    private const string AppDataFolderName = @"Open DCS Launcher";
    private const string SettingsFileName = @"settings.toml";

    public SettingsModel? Settings { get; private set; }

    private string GetSettingsFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsFolderPath = $"{appDataPath}\\{AppDataFolderName}";

        // We need to ensure that our folder path exists and create it if it does not.
        if (!Directory.Exists(settingsFolderPath))
        {
            Directory.CreateDirectory(settingsFolderPath);
        }

        return $"{appDataPath}\\{AppDataFolderName}\\{SettingsFileName}";
    }

    public async Task Load()
    {
        var settingsFilePath = GetSettingsFilePath();

        if (!File.Exists(settingsFilePath))
        {
            Settings = new SettingsModel();
            return;
        }

        var data = await File.ReadAllTextAsync(settingsFilePath);
        var settings = Toml.ToModel<SettingsModel>(data);

        Settings = settings;

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<bool> Save()
    {
        if (Settings == null) return false;

        var settingsFilePath = GetSettingsFilePath();
        var data = Toml.FromModel(Settings);

        await using var stream = File.CreateText(settingsFilePath);
        await stream.WriteAsync(data);
        await stream.FlushAsync();

        SettingsChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public event EventHandler? SettingsChanged;
}