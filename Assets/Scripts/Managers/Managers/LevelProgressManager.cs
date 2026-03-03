using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour, ISaveable
{
    public static LevelProgressManager Instance;

    public List<LevelSO> AllLevels = new();  // Assign in inspector
    private Dictionary<string, LevelProgress> progress = new();

    public string SaveKey => "LevelProgress";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveManager.Instance.Register(this);
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        foreach (var level in AllLevels)
        {
            if (!progress.ContainsKey(level.SceneName))
            {
                progress[level.SceneName] = new LevelProgress
                {
                    LevelID = level.SceneName,
                    Unlocked = false,
                    Played = false,
                    BestTime = -1,
                    BestRank = Rank.None
                };
            }
        }

        // Unlock first level by default
        if (AllLevels.Count > 0)
            progress[AllLevels[0].SceneName].Unlocked = true;
    }

    public LevelProgress GetProgress(LevelSO level)
    {
        progress.TryGetValue(level.SceneName, out var data);
        return data;
    }

    public void UpdateProgress(LevelSO level, float time, Rank rank)
    {
        var p = progress[level.SceneName];

        p.Played = true;

        if (p.BestTime <= 0 || time < p.BestTime)
            p.BestTime = time;

        if (rank < p.BestRank || p.BestRank == Rank.None)
            p.BestRank = rank;
    }

    public void UnlockNextImplementedLevel(LevelSO completedLevel)
    {
        if (completedLevel == null) return;

        int index = AllLevels.IndexOf(completedLevel);
        if (index < 0) return;

        // Safeguard: unlock all previous levels
        for (int i = 0; i <= index; i++)
        {
            var lvl = AllLevels[i];
            progress[lvl.SceneName].Unlocked = true;
        }

        // Find next implemented level
        for (int i = index + 1; i < AllLevels.Count; i++)
        {
            var next = AllLevels[i];

            if (next.IsImplemented)
            {
                progress[next.SceneName].Unlocked = true;
                break;
            }
        }
    }

    public void RepairUnlockOrder()
    {
        bool foundLocked = false;

        foreach (var level in AllLevels)
        {
            var p = progress[level.SceneName];

            if (!p.Unlocked)
                foundLocked = true;

            if (foundLocked)
                p.Unlocked = false;
        }
    }

    // ---------------- SAVE ----------------

    public Dictionary<string, string> CaptureSaveData()
    {
        Dictionary<string, string> data = new();

        foreach (var kvp in progress)
        {
            var p = kvp.Value;
            data[$"{p.LevelID}_Unlocked"] = SaveUtils.Bool(p.Unlocked);
            data[$"{p.LevelID}_Played"] = SaveUtils.Bool(p.Played);
            data[$"{p.LevelID}_BestTime"] = SaveUtils.Float(p.BestTime);
            data[$"{p.LevelID}_BestRank"] = SaveUtils.EnumToString(p.BestRank);
        }

        return data;
    }

    public void RestoreSaveData(Dictionary<string, string> data)
    {
        foreach (var key in new List<string>(progress.Keys))
        {
            var p = progress[key];

            if (data.TryGetValue($"{key}_Unlocked", out var u))
                p.Unlocked = SaveUtils.ToBool(u);

            if (data.TryGetValue($"{key}_Played", out var pl))
                p.Played = SaveUtils.ToBool(pl);

            if (data.TryGetValue($"{key}_BestTime", out var t))
                p.BestTime = SaveUtils.ToFloat(t);

            if (data.TryGetValue($"{key}_BestRank", out var r))
                p.BestRank = SaveUtils.StringToEnum<Rank>(r);
        }
    }
}