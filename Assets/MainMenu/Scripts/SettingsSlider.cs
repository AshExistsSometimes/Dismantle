using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class SettingsSlider : MonoBehaviour
{
    [Header("Binding")]
    public SettingsFloatKey settingKey;

    [Header("References")]
    public Slider slider;
    public TMP_Text label;

    [Header("Range")]
    public float minValue = 0f;
    public float maxValue = 1f;

    private bool suppressCallback;

    ////////////////////////////////////////////////////////
    // Unity

    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        slider.minValue = minValue;
        slider.maxValue = maxValue;

        slider.onValueChanged.AddListener(OnSliderChanged);

        // Automatically label from enum
        if (label != null)
            label.text = GetPrettyName(settingKey);
    }

    private void Start()
    {
        LoadFromSettings();
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    ////////////////////////////////////////////////////////
    // Logic

    private void LoadFromSettings()
    {
        suppressCallback = true;
        slider.value = GetValue();
        suppressCallback = false;
    }

    private void OnSliderChanged(float value)
    {
        if (suppressCallback)
            return;

        SetValue(value);
        SettingsManager.Instance.ApplyAllSettings();
        SettingsManager.Instance.SaveSettings();
    }

    ////////////////////////////////////////////////////////
    // Settings Access

    private float GetValue()
    {
        var s = SettingsManager.Instance.CurrentSettings;

        return settingKey switch
        {
            SettingsFloatKey.MasterVolume => s.masterVolume,
            SettingsFloatKey.MusicVolume => s.musicVolume,
            SettingsFloatKey.SFXVolume => s.sfxVolume,
            SettingsFloatKey.UIVolume => s.uiVolume,

            SettingsFloatKey.CrosshairSize => s.crosshairSize,
            SettingsFloatKey.CrosshairThickness => s.crosshairThickness,

            SettingsFloatKey.MouseSensitivity => s.mouseSensitivity,

            _ => 0f
        };
    }

    private void SetValue(float value)
    {
        var s = SettingsManager.Instance.CurrentSettings;

        switch (settingKey)
        {
            case SettingsFloatKey.MasterVolume:
                s.masterVolume = value;
                break;
            case SettingsFloatKey.MusicVolume:
                s.musicVolume = value;
                break;
            case SettingsFloatKey.SFXVolume:
                s.sfxVolume = value;
                break;
            case SettingsFloatKey.UIVolume:
                s.uiVolume = value;
                break;
            case SettingsFloatKey.CrosshairSize:
                s.crosshairSize = value;
                break;
            case SettingsFloatKey.CrosshairThickness:
                s.crosshairThickness = value;
                break;
            case SettingsFloatKey.MouseSensitivity:
                s.mouseSensitivity = value;
                break;
        }
    }

    ////////////////////////////////////////////////////////
    // Helpers

    private string GetPrettyName(SettingsFloatKey key)
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
