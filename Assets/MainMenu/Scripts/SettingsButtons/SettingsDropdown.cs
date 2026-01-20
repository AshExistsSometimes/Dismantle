using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SettingsDropdown : MonoBehaviour
{
    [Header("Binding")]
    public SettingsDropdownKey settingKey;

    [Header("References")]
    public TMP_Dropdown dropdown;
    public TMP_Text label;

    private bool suppressCallback;
    private readonly List<Resolution> cachedResolutions = new();

    ////////////////////////////////////////////////////////
    // Unity

    private void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponentInChildren<TMP_Dropdown>();

        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (label != null)
            label.text = GetPrettyName(settingKey);

        PopulateOptions();
    }

    private void Start()
    {
        LoadFromSettings();
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    ////////////////////////////////////////////////////////
    // Init

    private void PopulateOptions()
    {
        dropdown.ClearOptions();

        List<string> options = settingKey switch
        {
            SettingsDropdownKey.Resolution => GetResolutionOptions(),
            SettingsDropdownKey.FrameRateLimit => GetFrameRateOptions(),
            SettingsDropdownKey.GraphicsQuality => GetGraphicsQualityOptions(),
            _ => new List<string>()
        };

        dropdown.AddOptions(options);
    }

    private void LoadFromSettings()
    {
        suppressCallback = true;

        int value = GetValue();
        dropdown.value = Mathf.Clamp(value, 0, dropdown.options.Count - 1);
        dropdown.RefreshShownValue();

        suppressCallback = false;
    }

    ////////////////////////////////////////////////////////
    // Change

    private void OnDropdownChanged(int index)
    {
        if (suppressCallback)
            return;

        SetValue(index);
        SettingsManager.Instance.ApplyAllSettings();
        SettingsManager.Instance.SaveSettings();
    }

    ////////////////////////////////////////////////////////
    // Settings Access

    private int GetValue()
    {
        var s = SettingsManager.Instance.CurrentSettings;

        return settingKey switch
        {
            SettingsDropdownKey.Resolution => s.resolutionIndex,
            SettingsDropdownKey.FrameRateLimit => s.frameRateLimit,
            SettingsDropdownKey.GraphicsQuality => s.graphicsQualityIndex,
            _ => 0
        };
    }

    private void SetValue(int index)
    {
        var s = SettingsManager.Instance.CurrentSettings;

        switch (settingKey)
        {
            case SettingsDropdownKey.Resolution:
                s.resolutionIndex = index;
                ApplyResolution(index);
                break;

            case SettingsDropdownKey.FrameRateLimit:
                s.frameRateLimit = index;
                ApplyFrameRateLimit(index);
                break;

            case SettingsDropdownKey.GraphicsQuality:
                s.graphicsQualityIndex = index;
                QualitySettings.SetQualityLevel(index, true);
                break;
        }
    }

    ////////////////////////////////////////////////////////
    // Apply Logic

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= cachedResolutions.Count)
            return;

        Resolution r = cachedResolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    private void ApplyFrameRateLimit(int index)
    {
        int[] limits = GetFrameRateValues();

        int limit = limits[index];
        Application.targetFrameRate = limit <= 0 ? -1 : limit;
    }

    ////////////////////////////////////////////////////////
    // Option Builders

    private List<string> GetResolutionOptions()
    {
        cachedResolutions.Clear();
        List<string> options = new();

        Resolution[] all = Screen.resolutions;
        Resolution native = Screen.currentResolution;

        float targetAspect = 16f / 9f;
        int targetRefresh = native.refreshRate;

        for (int i = 0; i < all.Length; i++)
        {
            Resolution r = all[i];

            // Refresh rate filter
            if (r.refreshRate != targetRefresh)
                continue;

            // Aspect ratio filter (16:9 only)
            float aspect = (float)r.width / r.height;
            if (Mathf.Abs(aspect - targetAspect) > 0.01f)
                continue;

            // Deduplicate by resolution
            bool exists = false;
            for (int j = 0; j < cachedResolutions.Count; j++)
            {
                if (cachedResolutions[j].width == r.width &&
                    cachedResolutions[j].height == r.height)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
                continue;

            cachedResolutions.Add(r);

            bool isNative =
                r.width == native.width &&
                r.height == native.height;

            options.Add(
                isNative
                    ? $"{r.width} x {r.height} (Native)"
                    : $"{r.width} x {r.height}"
            );
        }

        return options;
    }

    private List<string> GetFrameRateOptions()
    {
        int[] limits = GetFrameRateValues();
        List<string> options = new();

        for (int i = 0; i < limits.Length; i++)
            options.Add(limits[i] <= 0 ? "Unlimited" : limits[i].ToString());

        return options;
    }

    private int[] GetFrameRateValues()
    {
        return new int[]
        {
            -1, // Unlimited
            30,
            60,
            120,
            144,
            165,
            240
        };
    }

    private List<string> GetGraphicsQualityOptions()
    {
        return new List<string>(QualitySettings.names);
    }

    ////////////////////////////////////////////////////////
    // Helpers

    private string GetPrettyName(SettingsDropdownKey key)
    {
        string raw = key.ToString();

        return Regex.Replace(
            raw,
            @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])",
            " "
        );
    }
}
