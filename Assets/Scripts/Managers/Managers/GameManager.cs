using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, ISaveable
{
    public static GameManager Instance;

    [Header("Global State")]
    public bool IsPaused;
    public bool UIOpen;
    public bool PlayerDead;

    [Header("Progress")]
    public List<string> LevelsUnlocked = new();

    [Header("Stats")]
    public float TotalPlaytime;
    public string CurrentDate;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveManager.Instance.Register("GameManager", this);
        CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
    }

    private void Update()
    {
        if (!IsPaused)
            TotalPlaytime += Time.deltaTime;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<string> levelsUnlocked;
        public float totalPlaytime;
        public string date;
    }

    public object SaveState()
    {
        return new SaveData
        {
            levelsUnlocked = LevelsUnlocked,
            totalPlaytime = TotalPlaytime,
            date = CurrentDate
        };
    }

    public void LoadState(object data)
    {
        var save = (SaveData)data;
        LevelsUnlocked = save.levelsUnlocked;
        TotalPlaytime = save.totalPlaytime;
        CurrentDate = save.date;
    }
}
