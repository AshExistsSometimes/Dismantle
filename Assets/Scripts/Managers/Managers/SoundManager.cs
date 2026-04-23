using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Volumes")]
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float MusicVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;
    [Range(0f, 1f)] public float UIVolume = 1f;

    [Header("Music Sources")]
    [SerializeField] private AudioSource baseSource;
    [SerializeField] private AudioSource combatSource;

    [Header("Combat Settings")]
    public AnimationCurve CombatFadeIn;
    public AnimationCurve CombatFadeOut;
    public float CombatFadeTime = 2f;
    private float combatFadeValue = 0f;

    private Coroutine combatRoutine;

    public List<AudioSource> musicSources = new();
    public List<AudioSource> sfxSources = new();
    public List<AudioSource> uiSources = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            var s = SettingsManager.Instance.CurrentSettings;

            SetVolumes(
                s.masterVolume,
                s.musicVolume,
                s.sfxVolume,
                s.uiVolume
            );
        }
    }

    ////////////////////////////////////////////////////////
    // REGISTRATION

    public void Register(AudioSource source, AudioType type)
    {
        if (source == null) return;

        switch (type)
        {
            case AudioType.Music:
                if (!musicSources.Contains(source))
                    musicSources.Add(source);
                break;

            case AudioType.SFX:
                if (!sfxSources.Contains(source))
                    sfxSources.Add(source);
                break;

            case AudioType.UI:
                if (!uiSources.Contains(source))
                    uiSources.Add(source);
                break;
        }

        ApplyVolume(source, type);
    }

    public void Unregister(AudioSource source)
    {
        musicSources.Remove(source);
        sfxSources.Remove(source);
        uiSources.Remove(source);
    }

    ////////////////////////////////////////////////////////
    // VOLUME

    public void RefreshVolumes()
    {
        foreach (var s in musicSources)
            ApplyVolume(s, AudioType.Music);

        foreach (var s in sfxSources)
            ApplyVolume(s, AudioType.SFX);

        foreach (var s in uiSources)
            ApplyVolume(s, AudioType.UI);
    }

    private void ApplyVolume(AudioSource source, AudioType type)
    {
        float vol = 1f;

        switch (type)
        {
            case AudioType.Music:
                vol = MusicVolume;
                break;
            case AudioType.SFX:
                vol = SFXVolume;
                break;
            case AudioType.UI:
                vol = UIVolume;
                break;
        }

        source.volume = vol * MasterVolume;
    }

    ////////////////////////////////////////////////////////
    // MUSIC SETUP

    public void SetLevelMusic(AudioClip baseClip, AudioClip combatClip)
    {
        if (baseSource == null || combatSource == null)
        {
            Debug.LogError("SoundManager missing AudioSources.");
            return;
        }

        baseSource.clip = baseClip;
        combatSource.clip = combatClip;

        baseSource.loop = true;
        combatSource.loop = true;

        // Start BOTH at the exact same DSP time (perfect sync)
        double startTime = AudioSettings.dspTime + 0.1f;

        baseSource.PlayScheduled(startTime);

        if (combatClip != null)
            combatSource.PlayScheduled(startTime);

        // Start muted
        combatSource.volume = 0f;
        combatSource.mute = true;
    }

    public void SetVolumes(float master, float music, float sfx, float ui)
    {
        MasterVolume = master;
        MusicVolume = music;
        SFXVolume = sfx;
        UIVolume = ui;

        RefreshVolumes();

        UpdateMusicVolumes();
    }

    private void UpdateMusicVolumes()
    {
        // Base track is always full music volume
        if (baseSource != null)
        {
            baseSource.volume = MusicVolume * MasterVolume;
        }

        if (combatSource != null)
        {
            // If muted, force volume to 0
            if (combatSource.mute)
            {
                combatSource.volume = 0f;
                return;
            }

            // Apply fade value properly
            combatSource.volume = combatFadeValue * MusicVolume * MasterVolume;
        }
    }

    ////////////////////////////////////////////////////////
    // COMBAT MUSIC

    public void FadeInCombatMusic()
    {
        if (combatRoutine != null)
            StopCoroutine(combatRoutine);

        combatRoutine = StartCoroutine(FadeCombat(CombatFadeIn, true));
    }

    public void FadeOutCombatMusic()
    {
        if (combatRoutine != null)
            StopCoroutine(combatRoutine);

        combatRoutine = StartCoroutine(FadeCombat(CombatFadeOut, false));
    }

    private IEnumerator FadeCombat(AnimationCurve curve, bool fadeIn)
    {
        combatSource.mute = false;

        float time = 0f;

        while (time < CombatFadeTime)
        {
            float t = time / CombatFadeTime;
            float value = curve.Evaluate(t);

            combatSource.volume = value * MusicVolume * MasterVolume;

            time += Time.deltaTime;
            yield return null;
        }

        if (!fadeIn)
        {
            combatSource.volume = 0f;
            combatSource.mute = true;
        }
    }
}

public enum AudioType
{
    Music,
    SFX,
    UI
}
