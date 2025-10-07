using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Pigeon.Movement;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SpeedometerMod : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.speedometer";
    public const string PluginName = "Speedometer";
    public const string PluginVersion = "1.0.2";

    private Text speedText;
    private Image backgroundImage;
    private Canvas uiCanvas;
    private FieldInfo currentMoveSpeedField;
    private FieldInfo moveVelocityField;
    private bool hasLoggedUpdateOnce = false;
    private bool hasLoggedScene = false;
    private bool uiVisible = true;

    private bool wasLocked = false;

    private static readonly Color sky = new Color(0.529f, 0.808f, 0.922f);

    private void Awake()
    {
        var harmony = new Harmony(PluginGUID);
        harmony.PatchAll();
        Logger.LogInfo($"{PluginName} loaded successfully.");

        currentMoveSpeedField = typeof(Player).GetField("currentMoveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        moveVelocityField = typeof(Player).GetField("moveVelocity", BindingFlags.NonPublic | BindingFlags.Instance);

        if (currentMoveSpeedField == null || moveVelocityField == null)
        {
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        CreateUI();
    }

    private void CreateUI()
    {
        if (uiCanvas != null) return;

        GameObject canvasGO = new GameObject("SpeedometerCanvas");
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 9999;
        uiCanvas.pixelPerfect = true;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.scaleFactor = 1f;

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject bgGO = new GameObject("SpeedBg");
        bgGO.transform.SetParent(canvasGO.transform, false);
        backgroundImage = bgGO.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.6f);
        backgroundImage.raycastTarget = false;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.11f, 1f);
        bgRect.anchorMax = new Vector2(0.11f, 1f);
        bgRect.anchoredPosition = new Vector2(0f, -15f);
        bgRect.sizeDelta = new Vector2(150f, 25f);

        GameObject textGO = new GameObject("SpeedText");
        textGO.transform.SetParent(bgGO.transform, false);
        speedText = textGO.AddComponent<Text>();
        speedText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speedText.text = "Speed: Loading...";
        speedText.fontSize = 15;
        speedText.color = Color.white;
        speedText.alignment = TextAnchor.MiddleCenter;
        speedText.raycastTarget = false;
        speedText.verticalOverflow = VerticalWrapMode.Overflow;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(1f, 1f);
        textRect.offsetMax = new Vector2(-1f, -1f);

        DontDestroyOnLoad(canvasGO);
    }

    private void Update()
    {
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            uiVisible = !uiVisible;
            if (backgroundImage != null) backgroundImage.enabled = uiVisible;
            if (speedText != null) speedText.enabled = uiVisible;
        }

        if (!uiVisible || speedText == null || Player.LocalPlayer == null)
        {
            if (speedText != null && uiVisible) speedText.text = "No Player";
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (!hasLoggedScene)
        {
            hasLoggedScene = true;
        }

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
                speed = new Vector3(mv.x, 0f, mv.z).magnitude;
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

        if (!hasLoggedUpdateOnce && speed > 0f)
        {
            hasLoggedUpdateOnce = true;
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

        bool isNowLocked = (Cursor.lockState == CursorLockMode.Locked);
    }

    private void OnDestroy()
    {
        if (uiCanvas != null) Destroy(uiCanvas.gameObject);
    }
}