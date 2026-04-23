using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Level")]
public class LevelSO : ScriptableObject
{
    [Header("Scene")]
    public string SceneName;

    [Header("Enemies")]
    public int EnemiesInLevel;

    [Header("Music")]
    public AudioClip BaseMusic;
    public AudioClip CombatMusic;

    [Header("UI")]
    public string LevelName;
    public Sprite LevelIcon;

    [Header("Time Rank Thresholds (seconds)")]
    public List<RankTimeFrame> TimeRanks = new();

    [Header("Score Rank Thresholds")]
    public List<RankScoreFrame> ScoreRanks = new();

    [Header("Development")]
    public bool IsImplemented = true; // false = Under Development
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

