using System;
using UnityEngine;

[Serializable]
public class UserSettingsData
{
    // Edit these to edit the default settings

    // Audio
    public float masterVolume = 0.5f;
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;
    public float uiVolume = 0.5f;

    // Video
    public int displayModeIndex = 0;
    public int resolutionIndex = 19;
    public bool vSyncEnabled = true;
    public bool fullscreen = true;
    public int frameRateLimit = 0; // 0 = unlimited
    public int graphicsQualityIndex = 2;

    // UI
    public float crosshairSize = 1f;
    public float crosshairThickness = 1f;
    public Color menuAccentColour = Color.red;

    // Gameplay
    public float mouseSensitivity = 85f;
}

