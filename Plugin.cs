using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // For new Input System
using Pigeon.Movement; // From Player.cs

[BepInPlugin("com.yourname.speedometer", "Speedometer Mod", "1.0.0")]
[MycoMod(null, ModFlags.IsClientSide)]
public class SpeedometerMod : BaseUnityPlugin
{
    private Text speedText;
    private Image backgroundImage; // For visibility test
    private Canvas uiCanvas;
    private FieldInfo currentMoveSpeedField;
    private FieldInfo moveVelocityField; // Fallback
    private bool hasLoggedUpdateOnce = false; // For one-time logging
    private bool hasLoggedScene = false; // For scene logging
    private bool uiVisible = true; // For F9 toggle

    private bool wasLocked = false; // For cursor lock tracking

    private void Awake()
    {
        var harmony = new Harmony("com.yourname.speedometer");
        harmony.PatchAll();

        currentMoveSpeedField = typeof(Player).GetField("currentMoveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        moveVelocityField = typeof(Player).GetField("moveVelocity", BindingFlags.NonPublic | BindingFlags.Instance);

        if (currentMoveSpeedField == null || moveVelocityField == null)
        {
            Logger.LogError("Failed to reflect Player fields! Mod disabled.");
            enabled = false;
            return;
        }

        Logger.LogInfo("Speedometer Mod Awake() - UI creation queued.");
    }

    private void Start()
    {
        CreateUI();
        Logger.LogInfo("Speedometer Mod Start() - UI created.");
    }

    private void CreateUI()
    {
        if (uiCanvas != null) return; // Avoid duplicates

        GameObject canvasGO = new GameObject("SpeedometerCanvas");
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // Explicit overlay
        uiCanvas.sortingOrder = 9999; // Ultra high to avoid occlusion
        uiCanvas.pixelPerfect = true; // Crisp in builds

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.scaleFactor = 1f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Larger background for testing (semi-transparent black rect)
        GameObject bgGO = new GameObject("SpeedBg");
        bgGO.transform.SetParent(canvasGO.transform, false);
        backgroundImage = bgGO.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0f); // 50% opaque black
        backgroundImage.raycastTarget = false;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.05f, 0.5f); // Top-center anchor
        bgRect.anchorMax = new Vector2(0.05f, 0.5f);
        bgRect.anchoredPosition = new Vector2(0f, -30f); // Slight downward offset from top-center; tweak if needed
        bgRect.sizeDelta = new Vector2(250f, 40f); // Bigger for full text

        // Text
        GameObject textGO = new GameObject("SpeedText");
        textGO.transform.SetParent(bgGO.transform, false); // Parent to bg
        speedText = textGO.AddComponent<Text>();
        speedText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speedText.text = "Speed: Loading...";
        speedText.fontSize = 20; // Slightly larger
        speedText.color = Color.white;
        speedText.alignment = TextAnchor.MiddleCenter; // Center for top-center pos
        speedText.raycastTarget = false; // No input blocking
        speedText.verticalOverflow = VerticalWrapMode.Overflow; // Allow wrap if needed
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 5f); // Padding
        textRect.offsetMax = new Vector2(-10f, -5f);

        DontDestroyOnLoad(canvasGO);
        //Logger.LogInfo("UI components added - check for larger black box top-center.");
    }

    private void Update()
    {
        // F9 toggle (using new Input System)
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            uiVisible = !uiVisible;
            if (backgroundImage != null) backgroundImage.enabled = uiVisible;
            if (speedText != null) speedText.enabled = uiVisible;
            Logger.LogInfo($"Speedometer UI {(uiVisible ? "shown" : "hidden")}.");
        }

        if (!uiVisible || speedText == null || Player.LocalPlayer == null)
        {
            if (speedText != null && uiVisible) speedText.text = "No Player";
            return;
        }

        // Log scene once for debugging
        string sceneName = SceneManager.GetActiveScene().name;
        if (!hasLoggedScene)
        {
            //Logger.LogInfo($"Current Scene Name (for filter tuning): {sceneName}");
            hasLoggedScene = true;
        }

        // Always try speed calc (relaxed filter)
        float speed = 0f;
        object speedObj = currentMoveSpeedField.GetValue(Player.LocalPlayer);
        if (speedObj is float cms)
        {
            speed = cms;
        }
        else
        {
            object velObj = moveVelocityField.GetValue(Player.LocalPlayer);
            if (velObj is Vector3 mv)
            {
                speed = new Vector3(mv.x, 0f, mv.z).magnitude; // Horizontal only
            }
        }

        if (speed > 0f)
        {
            speedText.text = $"Speed: {speed:F1} m/s";
        }
        else
        {
            speedText.text = "No Speed Detected"; // Simpler fallback
        }

        // One-time log for first valid update
        if (!hasLoggedUpdateOnce && speed > 0f)
        {
            //Logger.LogInfo($"First speed update - Value: {speed} (Scene: {sceneName})");
            hasLoggedUpdateOnce = true;
        }
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Track previous state before losing focus
            wasLocked = (Cursor.lockState == CursorLockMode.Locked);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // On focus gain: Always start with unlocked/visible to reset any stuck state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // If previously in gameplay (locked), player exists, and Tab menu is NOT open, relock
        if (wasLocked && Player.LocalPlayer != null && !Keyboard.current.tabKey.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        bool isNowLocked = (Cursor.lockState == CursorLockMode.Locked);
        //Logger.LogInfo($"App Focus: {hasFocus} - Cursor {(isNowLocked ? "relocked" : "unlocked/visible")}. WasLocked: {wasLocked}, TabPressed: {Keyboard.current.tabKey.isPressed}");
    }

    private void OnDestroy()
    {
        if (uiCanvas != null) Destroy(uiCanvas.gameObject);
    }
}