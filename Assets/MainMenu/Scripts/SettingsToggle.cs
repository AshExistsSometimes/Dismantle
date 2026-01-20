using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class SettingsToggle : MonoBehaviour
{
    [Header("Binding")]
    public SettingsBoolKey settingKey;

    [Header("References")]
    public Toggle toggle;
    public TMP_Text label;

    private bool suppressCallback;

    ////////////////////////////////////////////////////////
    // Unity

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponentInChildren<Toggle>();

        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        toggle.onValueChanged.AddListener(OnToggleChanged);

        // Auto-label from enum (same logic as slider)
        if (label != null)
            label.text = GetPrettyName(settingKey);
    }

    private void Start()
    {
        LoadFromSettings();
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    ////////////////////////////////////////////////////////
    // Logic

    private void LoadFromSettings()
    {
        suppressCallback = true;
        toggle.isOn = GetValue();
        suppressCallback = false;
    }

    private void OnToggleChanged(bool value)
    {
        if (suppressCallback)
            return;

        SetValue(value);
        SettingsManager.Instance.ApplyAllSettings();
        SettingsManager.Instance.SaveSettings();
    }

    ////////////////////////////////////////////////////////
    // Settings Access

    private bool GetValue()
    {
        var s = SettingsManager.Instance.CurrentSettings;

        return settingKey switch
        {
            SettingsBoolKey.VSync => s.vSyncEnabled,
            SettingsBoolKey.Fullscreen => s.fullscreen,

            _ => false
        };
    }

    private void SetValue(bool value)
    {
        var s = SettingsManager.Instance.CurrentSettings;

        switch (settingKey)
        {
            case SettingsBoolKey.VSync:
                s.vSyncEnabled = value;
                break;

            case SettingsBoolKey.Fullscreen:
                s.fullscreen = value;
                break;
        }
    }

    ////////////////////////////////////////////////////////
    // Helpers

    private string GetPrettyName(SettingsBoolKey key)
    {
        string raw = key.ToString();

        string spaced = Regex.Replace(
            raw,
            @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])",
            " "
        );

        return spaced;
    }
}
