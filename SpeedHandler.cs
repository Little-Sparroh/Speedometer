using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using Pigeon.Movement;
using Sparroh.UI;

public class SpeedometerMod
{
    private ConfigEntry<bool> enableSpeedometerHUD;
    private ConfigEntry<float> speedometerAnchorX;
    private ConfigEntry<float> speedometerAnchorY;
    private ConfigColor valueColor;
    private HudHandle hud;

    private FieldInfo currentMoveSpeedField;
    private FieldInfo vkField;
    private FieldInfo rbField;
    private FieldInfo moveVelocityField;
    private PropertyInfo vkProp;
    private PropertyInfo rbProp;

    private readonly ConfigFile configFile;
    private readonly Harmony harmony;

    public static SpeedometerMod Instance { get; private set; }

    public SpeedometerMod(ConfigFile configFile, Harmony harmony)
    {
        this.configFile = configFile;
        this.harmony = harmony;

        Instance = this;

        try
        {
            enableSpeedometerHUD = configFile.Bind("General", "EnableSpeedometerHUD", true, "Enables the speedometer HUD display.");
            enableSpeedometerHUD.SettingChanged += OnEnableSpeedometerHUDChanged;

            speedometerAnchorX = configFile.Bind("HUD Positioning", "SpeedometerAnchorX", 0.06418981f, "X anchor position for Speedometer (0-1).");
            speedometerAnchorY = configFile.Bind("HUD Positioning", "SpeedometerAnchorY", 0.2298982f, "Y anchor position for Speedometer (0-1).");
            speedometerAnchorX.SettingChanged += OnAnchorChanged;
            speedometerAnchorY.SettingChanged += OnAnchorChanged;

            valueColor = ConfigColor.Bind(configFile, "Colors", "ValueColor", UIColors.Sky,
                "Rich-text value color for speed (hex RRGGBB or #RRGGBB).");


            currentMoveSpeedField = typeof(Player).GetField("currentMoveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            vkField = typeof(Player).GetField("velocity", BindingFlags.NonPublic | BindingFlags.Instance) ??
                      typeof(Player).GetField("velocity", BindingFlags.Public | BindingFlags.Instance);
            if (vkField == null)
            {
                vkProp = typeof(Player).GetProperty("velocity", BindingFlags.Public | BindingFlags.Instance);
            }
            if (vkField == null && vkProp == null)
            {
                rbField = typeof(Player).GetField("rb", BindingFlags.NonPublic | BindingFlags.Instance) ??
                          typeof(Player).GetField("rb", BindingFlags.Public | BindingFlags.Instance);
            }
            if (rbField == null && vkField == null && vkProp == null)
            {
                rbProp = typeof(Player).GetProperty("rb", BindingFlags.Public | BindingFlags.Instance);
            }
            if (rbField == null && rbProp == null && vkField == null && vkProp == null)
            {
                moveVelocityField = typeof(Player).GetField("moveVelocity", BindingFlags.NonPublic | BindingFlags.Instance) ??
                                    typeof(Player).GetField("moveVelocity", BindingFlags.Public | BindingFlags.Instance);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Failed to initialize Speedometer reflection: {ex.Message}");
        }
    }

    public bool IsActive => HudHandle.IsValid(hud) && hud.IsActive;
    public Vector2 GetSize => HudHandle.IsValid(hud) ? hud.Size : Vector2.zero;

    public void UpdateHudVisibility()
    {
        if (HudHandle.IsValid(hud))
            hud.SetActive(enableSpeedometerHUD.Value);
    }

    private void OnEnableSpeedometerHUDChanged(object sender, EventArgs e)
    {
        if (enableSpeedometerHUD.Value == false && HudHandle.IsValid(hud))
        {
            DestroyHud();
        }
        UpdateHudVisibility();
    }

    private void OnAnchorChanged(object sender, EventArgs e)
    {
        UpdateAnchors();
    }

    private void UpdateAnchors()
    {
        if (HudHandle.IsValid(hud))
            hud.SetAnchor(speedometerAnchorX.Value, speedometerAnchorY.Value);
    }

    private void CreateSpeedometerHUD()
    {
        // Stale handle after quit-to-menu: C# wrapper survives, GameObject does not.
        if (hud != null && !hud.IsAlive)
        {
            HudRepositionClient.Unregister(SparrohPlugin.PluginGUID);
            hud = null;
        }

        if (HudHandle.IsValid(hud)) return;

        hud = HudBuilder.Create("SpeedometerHUD")
            .ParentToReticle()
            .Anchor(speedometerAnchorX.Value, speedometerAnchorY.Value)
            .Pivot(new Vector2(0f, 0.5f))
            .Size(300f, 25f)
            .AddText("SpeedText")
            .Build();

        if (!HudHandle.IsValid(hud))
            return;

        HudRepositionClient.Register(
            SparrohPlugin.PluginGUID,
            "Speedometer",
            hud.Rect,
            speedometerAnchorX,
            speedometerAnchorY);

        UpdateHudVisibility();
    }

    private void DestroyHud()
    {
        HudRepositionClient.Unregister(SparrohPlugin.PluginGUID);
        if (hud != null)
        {
            if (hud.IsAlive)
                hud.Destroy();
            hud = null;
        }
    }

    public void Update()
    {
        try
        {
            if (!enableSpeedometerHUD.Value) return;

            if (!HudHandle.IsValid(hud))
            {
                CreateSpeedometerHUD();
                return;
            }

            if (hud.Primary == null || Player.LocalPlayer == null)
            {
                if (hud.Primary != null)
                    hud.Primary.Text = "No Player";
                return;
            }


            float speed = ReadSpeed();

            if (speed > 0f)
                hud.Primary.SetRich("Speed", speed, valueColor.Value, "m/s");
            else
                hud.Primary.Text = "No Speed Detected";

        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Error in Speedometer.Update(): {ex.Message}");
        }
    }

    private float ReadSpeed()
    {
        float speed = 0f;

        if (vkField != null || vkProp != null)
        {
            if (vkField != null)
            {
                object velObj = vkField.GetValue(Player.LocalPlayer);
                if (velObj is Vector3 vel)
                    speed = vel.magnitude;
            }
            else if (vkProp != null)
            {
                object velObj = vkProp.GetValue(Player.LocalPlayer);
                if (velObj is Vector3 vel)
                    speed = vel.magnitude;
            }
        }
        else if (rbField != null || rbProp != null)
        {
            if (rbField != null)
            {
                object rbObj = rbField.GetValue(Player.LocalPlayer);
                if (rbObj is Rigidbody rb)
                    speed = rb.velocity.magnitude;
            }
            else if (rbProp != null)
            {
                object rbObj = rbProp.GetValue(Player.LocalPlayer);
                if (rbObj is Rigidbody rb)
                    speed = rb.velocity.magnitude;
            }
        }

        if (speed == 0f && currentMoveSpeedField != null)
        {
            object cmsObj = currentMoveSpeedField.GetValue(Player.LocalPlayer);
            if (cmsObj is float cms)
                speed = cms;
        }

        if (speed == 0f && moveVelocityField != null)
        {
            object velObj = moveVelocityField.GetValue(Player.LocalPlayer);
            if (velObj is Vector3 mv)
                speed = mv.magnitude;
        }

        return speed;
    }

    public void OnDestroy()
    {
        try
        {
            DestroyHud();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Error in Speedometer.OnDestroy(): {ex.Message}");
        }
    }
}
