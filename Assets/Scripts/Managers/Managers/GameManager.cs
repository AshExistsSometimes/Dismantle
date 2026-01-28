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
    public List<LevelProgress> LevelProgression = new();

    [Header("Stats")]
    public float TotalPlaytime;
    public string CurrentDate;

    [Header("Player References")]
    public PlayerController playerController;

    public string SaveKey => "GameManager";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");

        SaveManager.Instance?.Register(this);
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
            Debug.Log("[GameManager] Registered with SaveManager.");
        }
        else
        {
            Debug.LogError("[GameManager] SaveManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }

    private void Update()
    {
        if (!IsPaused)
            TotalPlaytime += Time.deltaTime;
    }
    
    // Saving //
    public Dictionary<string, string> CaptureSaveData()
    {
        Dictionary<string, string> data = new();

        data["CurrentPlaytime"] =
            TimeSpan.FromSeconds(TotalPlaytime).ToString(@"hh\:mm\:ss");

        data["CurrentDate"] = CurrentDate;

        if (LevelProgression.Count == 0)
        {
            data["Levels"] = "(none)";
        }
        else
        {
            List<string> levelLines = new();

            foreach (var level in LevelProgression)
            {
                if (!level.Unlocked)
                    continue;

                if (!level.Played)
                {
                    levelLines.Add($"{level.LevelID} - Unplayed");
                }
                else
                {
                    string time = TimeSpan
                        .FromSeconds(level.BestTime)
                        .ToString(@"mm\:ss\.ff");

                    levelLines.Add(
                        $"{level.LevelID} - BestTime = {time} | Rank = {level.BestRank}"
                    );
                }
            }

            data["Levels"] = string.Join("\n", levelLines);
        }

        return data;
    }

    public void RestoreSaveData(Dictionary<string, string> data)
    {
        if (data.TryGetValue("CurrentPlaytime", out var timeString))
        {
            if (TimeSpan.TryParse(timeString, out var time))
                TotalPlaytime = (float)time.TotalSeconds;
        }

        if (data.TryGetValue("CurrentDate", out var date))
        {
            CurrentDate = date;
        }

        if (!data.TryGetValue("Levels", out var levelsBlock))
            return;

        LevelProgression.Clear();

        string[] lines = levelsBlock.Split('\n');

        foreach (string line in lines)
        {
            if (line.Contains("Unplayed"))
            {
                string id = line.Split(" - ")[0];

                LevelProgression.Add(new LevelProgress
                {
                    LevelID = id,
                    Unlocked = true,
                    Played = false
                });
            }
            else
            {
                // 0.1_Tutorial - BestTime = 00:07:23.53 | Rank = A

                string[] parts = line.Split(" - ");
                string id = parts[0];

                string[] values = parts[1].Split('|');

                float bestTime = 0f;
                Rank bestRank = Rank.F;

                foreach (var v in values)
                {
                    if (v.Contains("BestTime"))
                    {
                        string timeStr = v.Split('=')[1].Trim();
                        if (TimeSpan.TryParse(timeStr, out var ts))
                            bestTime = (float)ts.TotalSeconds;
                    }
                    else if (v.Contains("Rank"))
                    {
                        Enum.TryParse(v.Split('=')[1].Trim(), out bestRank);
                    }
                }

                LevelProgression.Add(new LevelProgress
                {
                    LevelID = id,
                    Unlocked = true,
                    Played = true,
                    BestTime = bestTime,
                    BestRank = bestRank
                });
            }
        }
    }

    // Dialogue //

    public void EnterDialogue()
    {
        UIOpen = true;
        IsPaused = true;

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitDialogue()
    {
        UIOpen = false;
        IsPaused = false;

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
