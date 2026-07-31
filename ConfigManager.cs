using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Sparroh.UI;
using UnityEngine;

public static class ConfigManager
{
    private const float DebounceSeconds = 0.25f;

    private static ConfigFile config;
    private static ManualLogSource logger;
    private static FileSystemWatcher configWatcher;
    private static volatile bool pendingRefresh;
    private static volatile bool reloadPending;
    private static float lastReloadTime;
    public static ConfigEntry<bool> EnableSpeedometerHUD { get; private set; }
    public static HudAnchors Anchors { get; private set; }
    public static ConfigColor ValueColor { get; private set; }

    public static void Initialize(ConfigFile configFile, ManualLogSource log)
    {
        config = configFile;
        logger = log;

        EnableSpeedometerHUD = config.Bind(
            "General",
            "Enable Speedometer",
            true,
            "Enables the speedometer HUD display.");

        Anchors = HudAnchors.Bind(config, "Speedometer", 0.06418981f, 0.2298982f);

        ValueColor = ConfigColor.Bind(
            config,
            "Colors",
            "Speed Color",
            UIColors.Sky,
            "Rich-text value color for speed (hex RRGGBB or #RRGGBB).");

        EnableSpeedometerHUD.SettingChanged += OnSettingChanged;

        try
        {
            SetupFileWatcher();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error setting up config file watcher: {ex.Message}");
        }
    }


    public static void Tick()
    {
        if (!reloadPending)
            return;

        if (Time.unscaledTime - lastReloadTime < DebounceSeconds)
            return;

        reloadPending = false;
        lastReloadTime = Time.unscaledTime;

        try
        {
            config.Reload();
            pendingRefresh = true;
            logger.LogInfo("Config reloaded from disk.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reloading config: {ex.Message}");
        }
    }

    public static bool ConsumePendingRefresh()
    {
        if (!pendingRefresh)
            return false;

        pendingRefresh = false;
        return true;
    }

    public static void Dispose()
    {
        if (EnableSpeedometerHUD != null)
            EnableSpeedometerHUD.SettingChanged -= OnSettingChanged;

        if (configWatcher != null)
        {
            configWatcher.EnableRaisingEvents = false;
            configWatcher.Changed -= OnConfigFileChanged;
            configWatcher.Created -= OnConfigFileChanged;
            configWatcher.Renamed -= OnConfigFileChanged;
            configWatcher.Dispose();
            configWatcher = null;
        }
    }

    private static void SetupFileWatcher()
    {
        configWatcher = new FileSystemWatcher(Paths.ConfigPath, $"{SpeedometerPlugin.PluginGUID}.cfg");
        configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName;
        configWatcher.Changed += OnConfigFileChanged;
        configWatcher.Created += OnConfigFileChanged;
        configWatcher.Renamed += OnConfigFileChanged;
        configWatcher.EnableRaisingEvents = true;
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        reloadPending = true;
    }

    private static void OnSettingChanged(object sender, EventArgs e)
    {
        pendingRefresh = true;
    }
}