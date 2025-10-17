using BepInEx;
using HarmonyLib;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Pigeon.Movement;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SpeedometerMod : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.speedometer";
    public const string PluginName = "Speedometer";
    public const string PluginVersion = "1.1.0";

    private TextMeshProUGUI speedText;
    private GameObject hudContainer;
    private FieldInfo currentMoveSpeedField;
    private FieldInfo vkField;
    private FieldInfo rbField;
    private FieldInfo moveVelocityField;
    private PropertyInfo vkProp;
    private PropertyInfo rbProp;
    private bool uiVisible = true;
    private bool wasLocked = false;

    private static readonly Color sky = new Color(0.529f, 0.808f, 0.922f);

    private void Awake()
    {
        var harmony = new Harmony(PluginGUID);
        harmony.PatchAll();
        Logger.LogInfo($"{PluginName} loaded successfully.");

        currentMoveSpeedField = typeof(Player).GetField("currentMoveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

        vkField = typeof(Player).GetField("velocity", BindingFlags.NonPublic | BindingFlags.Instance) ??
                  typeof(Player).GetField("velocity", BindingFlags.Public | BindingFlags.Instance);

        if (vkField != null)
        {
            return;
        }

        vkProp = typeof(Player).GetProperty("velocity", BindingFlags.Public | BindingFlags.Instance);
        if (vkProp != null)
        {
            return;
        }

        rbField = typeof(Player).GetField("rb", BindingFlags.NonPublic | BindingFlags.Instance) ??
                  typeof(Player).GetField("rb", BindingFlags.Public | BindingFlags.Instance);

        if (rbField != null)
        {
            return;
        }

        rbProp = typeof(Player).GetProperty("rb", BindingFlags.Public | BindingFlags.Instance);
        if (rbProp != null)
        {
            return;
        }

        moveVelocityField = typeof(Player).GetField("moveVelocity", BindingFlags.NonPublic | BindingFlags.Instance) ??
                            typeof(Player).GetField("moveVelocity", BindingFlags.Public | BindingFlags.Instance);

        if (moveVelocityField != null)
        {
            return;
        }

    }

    private void CreateHUD()
    {
        if (hudContainer != null) return;

        if (Player.LocalPlayer == null || Player.LocalPlayer.PlayerLook == null || Player.LocalPlayer.PlayerLook.Reticle == null) return;

        var parent = Player.LocalPlayer.PlayerLook.Reticle;
        hudContainer = new GameObject("SpeedometerHUD");
        hudContainer.transform.SetParent(parent, false);

        var containerRect = hudContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.2f, 0.9f);
        containerRect.anchorMax = new Vector2(0.2f, 0.9f);
        containerRect.anchoredPosition = new Vector2(0f, 0f);
        containerRect.sizeDelta = new Vector2(300f, 25f);

        GameObject textGO = new GameObject("SpeedText");
        textGO.transform.SetParent(hudContainer.transform, false);
        speedText = textGO.AddComponent<TextMeshProUGUI>();
        speedText.fontSize = 18;
        speedText.color = Color.white;
        speedText.enableWordWrapping = false;
        speedText.alignment = TextAlignmentOptions.Left;
        speedText.verticalAlignment = VerticalAlignmentOptions.Middle;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = new Vector2(0f, 0f);
    }

    private void Update()
    {
        if (hudContainer == null)
        {
            CreateHUD();
            return;
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            uiVisible = !uiVisible;
            if (hudContainer != null) hudContainer.SetActive(uiVisible);
        }

        if (!uiVisible || hudContainer == null || speedText == null || Player.LocalPlayer == null)
        {
            if (speedText != null && uiVisible) speedText.text = "No Player";
            return;
        }

        GameObject damageMeterHUD = GameObject.Find("DamageMeterHUD");
        var containerRect = hudContainer.GetComponent<RectTransform>();
        containerRect.anchoredPosition = new Vector2(0f, (damageMeterHUD != null && damageMeterHUD.activeSelf) ? -100f : 0f);

        float speed = 0f;

        if (vkField != null || vkProp != null)
        {
            if (vkField != null)
            {
                object velObj = vkField.GetValue(Player.LocalPlayer);
                if (velObj is Vector3 vel)
                {
                    speed = vel.magnitude;
                }
            }
            else if (vkProp != null)
            {
                object velObj = vkProp.GetValue(Player.LocalPlayer);
                if (velObj is Vector3 vel)
                {
                    speed = vel.magnitude;
                }
            }
        }
        else if (rbField != null || rbProp != null)
        {
            if (rbField != null)
            {
                object rbObj = rbField.GetValue(Player.LocalPlayer);
                if (rbObj is Rigidbody rb)
                {
                    speed = rb.velocity.magnitude;
                }
            }
            else if (rbProp != null)
            {
                object rbObj = rbProp.GetValue(Player.LocalPlayer);
                if (rbObj is Rigidbody rb)
                {
                    speed = rb.velocity.magnitude;
                }
            }
        }

        if (speed == 0f && currentMoveSpeedField != null)
        {
            object cmsObj = currentMoveSpeedField.GetValue(Player.LocalPlayer);
            if (cmsObj is float cms)
            {
                speed = cms;
            }
        }

        if (speed == 0f && moveVelocityField != null)
        {
            object velObj = moveVelocityField.GetValue(Player.LocalPlayer);
            if (velObj is Vector3 mv)
            {
                speed = mv.magnitude;
            }
        }

        if (speed > 0f)
        {
            speedText.text = $"Speed: <color=#{ColorUtility.ToHtmlStringRGB(sky)}>{speed:F1}</color> m/s";
        }
        else
        {
            speedText.text = "No Speed Detected";
        }
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            wasLocked = (Cursor.lockState == CursorLockMode.Locked);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (wasLocked && Player.LocalPlayer != null && !Keyboard.current.tabKey.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDestroy()
    {
        if (hudContainer != null) Destroy(hudContainer);
    }
}
