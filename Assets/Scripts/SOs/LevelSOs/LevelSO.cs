using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Level")]
public class LevelSO : ScriptableObject
{
    [Header("Scene")]
    public string SceneName;

    [Header("Enemies")]
    public int EnemiesInLevel;

    [Header("Time Rank Thresholds (seconds)")]
    public List<RankTimeFrame> TimeRanks = new();

    [Header("Score Rank Thresholds")]
    public List<RankScoreFrame> ScoreRanks = new();

    [Header("Management")]
    public bool Unlocked = false;
    public bool Played = false;
}

[System.Serializable]
public struct RankTimeFrame
{
    public Rank Rank;
    public float MaxTime;
}

[System.Serializable]
public struct RankScoreFrame
{
    public Rank Rank;
    public int MinScore;
}

public enum Rank
{
    SS,
    S,
    A,
    B,
    C,
    D,
    E,
    F,
    None
}

