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
        // --------------------
        // Basic stats
        // --------------------

        if (data.TryGetValue("CurrentPlaytime", out var timeString))
        {
            if (TimeSpan.TryParse(timeString, out var time))
                TotalPlaytime = (float)time.TotalSeconds;
        }

        if (data.TryGetValue("CurrentDate", out var date))
        {
            CurrentDate = date;
        }

        // --------------------
        // Level progression
        // --------------------

        if (!data.TryGetValue("Levels", out var levelsBlock))
            return;

        LevelProgression.Clear();

        if (string.IsNullOrWhiteSpace(levelsBlock))
            return;

        string[] lines = levelsBlock.Split('\n');

        foreach (string line in lines)
        {
            // Skip empty or placeholder lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line == "(none)")
                continue;

            // --------------------
            // Unplayed level
            // --------------------

            if (line.Contains("Unplayed"))
            {
                string[] parts = line.Split(" - ");

                if (parts.Length < 1)
                    continue;

                LevelProgression.Add(new LevelProgress
                {
                    LevelID = parts[0],
                    Unlocked = true,
                    Played = false
                });

                continue;
            }

            // --------------------
            // Played level
            // Expected format:
            // ID - BestTime = xx | Rank = X
            // --------------------

            string[] playedParts = line.Split(" - ");

            if (playedParts.Length < 2)
            {
                Debug.LogWarning($"[GameManager] Invalid level save line skipped: '{line}'");
                continue;
            }

            string levelID = playedParts[0];
            string[] values = playedParts[1].Split('|');

            float bestTime = 0f;
            Rank bestRank = Rank.F;

            foreach (var value in values)
            {
                if (value.Contains("BestTime"))
                {
                    string[] split = value.Split('=');
                    if (split.Length > 1 && TimeSpan.TryParse(split[1].Trim(), out var ts))
                        bestTime = (float)ts.TotalSeconds;
                }
                else if (value.Contains("Rank"))
                {
                    string[] split = value.Split('=');
                    if (split.Length > 1)
                        Enum.TryParse(split[1].Trim(), out bestRank);
                }
            }

            LevelProgression.Add(new LevelProgress
            {
                LevelID = levelID,
                Unlocked = true,
                Played = true,
                BestTime = bestTime,
                BestRank = bestRank
            });
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
