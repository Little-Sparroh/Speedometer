using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("sparroh.uilibrary")]
[MycoMod(null, ModFlags.IsClientSide)]
public class SparrohPlugin : BaseUnityPlugin
{

    public const string PluginGUID = "sparroh.speedometer";
    public const string PluginName = "Speedometer";
    public const string PluginVersion = "1.2.1";

    internal static new ManualLogSource Logger;

    private Harmony harmony;
    private SpeedometerMod speedometer;

    private void Awake()
    {
        Logger = base.Logger;

        try
        {
            harmony = new Harmony(PluginGUID);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create Harmony instance: {ex.Message}");
            return;
        }

        var configFile = Config;
        try
        {
            var watcher = new FileSystemWatcher(Paths.ConfigPath, "sparroh.speedometer.cfg");
            watcher.Changed += (s, e) =>
            {
                configFile.Reload();
            };
            watcher.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to set up config watcher: {ex.Message}");
        }

        try
        {
            speedometer = new SpeedometerMod(configFile, harmony);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize Speedometer: {ex.Message}");
        }

        try
        {
            harmony.PatchAll();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply Harmony patches: {ex.Message}");
        }

        Logger.LogInfo($"{PluginName} loaded successfully.");
    }

    private void Update()
    {
        try
        {
            if (speedometer != null) speedometer.UpdateHudVisibility();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in Speedometer.UpdateHudVisibility(): {ex.Message}");
        }

        try
        {
            if (speedometer != null) speedometer.Update();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in Speedometer.Update(): {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        try
        {
            if (speedometer != null) speedometer.OnDestroy();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in Speedometer.OnDestroy(): {ex.Message}");
        }

        try
        {
            if (harmony != null) harmony.UnpatchSelf();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error unpatching Harmony: {ex.Message}");
        }
    }
}
