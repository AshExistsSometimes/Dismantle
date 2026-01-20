using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [HideInInspector]
    public float MouseSensitivity;

    public static SettingsManager Instance { get; private set; }

    public UserSettingsData CurrentSettings { get; private set; }

    private readonly List<MenuAccentReceiver> accentReceivers = new();

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "UserSettings.Set");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAllSettings();
    }

    ////////////////////////////////////////////////////////
    // Save / Load

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(SavePath, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            CurrentSettings = JsonUtility.FromJson<UserSettingsData>(json);
        }
        else
        {
            CurrentSettings = new UserSettingsData();

            // Default resolution = native
            Resolution native = Screen.currentResolution;

            CurrentSettings.resolutionIndex = 0; // fallback

            Resolution[] all = Screen.resolutions;

            float targetAspect = 16f / 9f;

            for (int i = 0; i < all.Length; i++)
            {
                float aspect = (float)all[i].width / all[i].height;

                if (Mathf.Abs(aspect - targetAspect) > 0.01f)
                    continue;

                if (all[i].width == native.width &&
                    all[i].height == native.height)
                {
                    CurrentSettings.resolutionIndex = i;
                    break;
                }
            }

            SaveSettings();
        }
    }

    ////////////////////////////////////////////////////////
    // Apply

    public void ApplyAllSettings()
    {
        ApplyAudio();
        ApplyVideo();
        ApplyGameplay();
        ApplyUI();
        ApplyMenu();
    }

    private void ApplyAudio()
    {
        // Hook into AudioMixer later
    }

    private void ApplyVideo()
    {
        // VSync
        QualitySettings.vSyncCount =
            CurrentSettings.vSyncEnabled ? 1 : 0;

        // Fullscreen
        Screen.fullScreen = CurrentSettings.fullscreen;

        // Frame rate limit
        if (CurrentSettings.frameRateLimit <= 0)
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = CurrentSettings.frameRateLimit;

        // Graphics quality
        QualitySettings.SetQualityLevel(
            CurrentSettings.graphicsQualityIndex,
            true
        );
    }

    private void ApplyGameplay()
    {
        MouseSensitivity = CurrentSettings.mouseSensitivity;
    }

    private void ApplyUI()
    {
        // Crosshair logic later
    }

    public void SetMenuAccentColour(Color colour)
    {
        CurrentSettings.menuAccentColour = colour;
        SaveSettings();
        ApplyMenu();
    }

    private void ApplyMenu()
    {
        for (int i = 0; i < accentReceivers.Count; i++)
        {
            if (accentReceivers[i] != null)
                accentReceivers[i].Apply(CurrentSettings.menuAccentColour);
        }
    }

    public void RegisterAccentReceiver(MenuAccentReceiver receiver)
    {
        if (!accentReceivers.Contains(receiver))
            accentReceivers.Add(receiver);

        receiver.Apply(CurrentSettings.menuAccentColour);
    }

    public static int GetNativeResolutionIndex(List<Resolution> filteredResolutions)
    {
        Resolution native = Screen.currentResolution;

        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            if (filteredResolutions[i].width == native.width &&
                filteredResolutions[i].height == native.height)
            {
                return i;
            }
        }

        return 0;
    }
}
