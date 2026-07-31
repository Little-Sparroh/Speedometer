using Pigeon.Movement;
using Sparroh.UI;
using UnityEngine;

public class SpeedometerMod
{
    private readonly PlayerSpeedReader speedReader = new();
    private HudHandle hud;

    public SpeedometerMod()
    {
        Instance = this;
    }

    public static SpeedometerMod Instance { get; private set; }

    public bool IsActive => HudHandle.IsValid(hud) && hud.IsActive;
    public Vector2 GetSize => HudHandle.IsValid(hud) ? hud.Size : Vector2.zero;

    public void OnConfigChanged()
    {
        if (!ConfigManager.EnableSpeedometerHUD.Value && HudHandle.IsValid(hud))
            DestroyHud();
        UpdateHudVisibility();
    }

    public void UpdateHudVisibility()
    {
        if (HudHandle.IsValid(hud))
            hud.SetActive(ConfigManager.EnableSpeedometerHUD.Value);
    }

    private void CreateSpeedometerHUD()
    {
        if (hud != null && !hud.IsAlive)
            hud = null;

        if (HudHandle.IsValid(hud)) return;

        hud = HudBuilder.Create("SpeedometerHUD")
            .ParentToReticle()
            .Anchor(ConfigManager.Anchors.XValue, ConfigManager.Anchors.YValue)
            .Pivot(new Vector2(0f, 0.5f))
            .Size(300f, 25f)
            .AddText("SpeedText")
            .Build();

        if (!HudHandle.IsValid(hud))
            return;

        hud.EnableReposition(SpeedometerPlugin.PluginGUID, "Speedometer", ConfigManager.Anchors);
        UpdateHudVisibility();
    }

    private void DestroyHud()
    {
        if (hud != null)
        {
            if (hud.IsAlive)
                hud.Destroy();
            hud = null;
        }
    }

    public void Update()
    {
        if (!ConfigManager.EnableSpeedometerHUD.Value) return;

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

        var speed = speedReader.Read(Player.LocalPlayer);

        if (speed > 0f)
            hud.Primary.SetRich("Speed", speed, ConfigManager.ValueColor.Value, "m/s");
        else
            hud.Primary.Text = "No Speed Detected";
    }

    public void OnDestroy()
    {
        DestroyHud();
    }
}