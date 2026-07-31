using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("sparroh.uilibrary")]
[MycoMod(null, ModFlags.IsClientSide)]
public class SpeedometerPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.speedometer";
    public const string PluginName = "Speedometer";
    public const string PluginVersion = "1.2.2";

    internal new static ManualLogSource Logger;

    private Harmony harmony;
    private SpeedometerMod speedometer;

    private void Awake()
    {
        Logger = base.Logger;

        try
        {
            ConfigManager.Initialize(Config, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize config: {ex.Message}");
            return;
        }

        try
        {
            harmony = new Harmony(PluginGUID);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create Harmony instance: {ex.Message}");
            return;
        }

        try
        {
            speedometer = new SpeedometerMod();
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
            ConfigManager.Tick();

            if (ConfigManager.ConsumePendingRefresh() && speedometer != null)
                speedometer.OnConfigChanged();

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
            ConfigManager.Dispose();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error disposing config: {ex.Message}");
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