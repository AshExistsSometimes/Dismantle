using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Selection")]
    public LevelSO SelectedLevel;

    [Header("Runtime Stats")]
    public float LevelTime { get; private set; }
    public int Score { get; private set; }
    public int EnemiesKilled { get; private set; }
    public int EnemiesInLevel { get; private set; }
    public int CollectedTickets { get; private set; }

    [Header("Levels Final Stats")]
    public float LevelEndTime;
    public float LevelEndScore;
    public int LevelEndKilCount;
    public int LevelEndTickets;

    [Header("State")]
    public bool TimerRunning { get; private set; }

    [Header("Ranks")]
    public Rank KillCountRank;
    public Rank TimeRank;
    public Rank ScoreRank;
    public Rank OverallRank;


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

    private void Update()
    {
        if (TimerRunning)
            LevelTime += Time.deltaTime;
    }

    // --------------------
    // Level Loading
    // --------------------

    public void LoadSelectedLevel(LevelSO level)
    {
        if (level == null)
        {
            Debug.LogError("LevelManager.LoadSelectedLevel called with null LevelSO.");
            return;
        }

        SelectedLevel = level;
        EnemiesInLevel = level.EnemiesInLevel;

        ResetLevelStats();

        SceneManager.LoadScene(level.SceneName);
    }

    public void LevelComplete()
    {
        StopTimer();
        LevelEndCaptureStats();

        CalculateRankings();

        LevelProgressManager.Instance.UnlockNextImplementedLevel(SelectedLevel);

        SaveManager.Instance.SaveGame();

        OpenLevelEndScreen();

        
    }

    public void ReturnToHub()
    {
        //LevelProgressManager.Instance.UpdateProgress(SelectedLevel, LevelEndTime, rank);
        SaveManager.Instance.SaveGame();

        SceneManager.LoadScene("0.0_Hub");
    }


    // --------------------
    // Timer
    // --------------------

    public void StartTimer()
    {
        TimerRunning = true;
    }

    public void StopTimer()
    {
        TimerRunning = false;
    }

    // --------------------
    // Stat Updates (called externally)
    // --------------------

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void EnemyWasKilled()
    {
        EnemiesKilled++;
    }

    public void AddTicket(int tickets)
    {
        CollectedTickets = (CollectedTickets + tickets);
        Debug.Log("New Ticket Amount: " + CollectedTickets);
    }

    // --------------------
    // Final Stat Calculations
    // --------------------

    private void CalculateRankings()
    {
        if (SelectedLevel == null)
        {
            Debug.LogError("CalculateRankings called with null SelectedLevel.");
            return;
        }

        int bonusTickets = 0;

        // ----------------------------
        // KILL COUNT RANK
        // ----------------------------

        float killRatio = SelectedLevel.EnemiesInLevel <= 0
            ? 1f
            : (float)EnemiesKilled / SelectedLevel.EnemiesInLevel;

        if (killRatio >= 1f) KillCountRank = Rank.S;
        else if (killRatio >= 0.9f) KillCountRank = Rank.A;
        else if (killRatio >= 0.8f) KillCountRank = Rank.B;
        else if (killRatio >= 0.6f) KillCountRank = Rank.C;
        else if (killRatio >= 0.3f) KillCountRank = Rank.D;
        else if (killRatio >= 0.2f) KillCountRank = Rank.E;
        else KillCountRank = Rank.F;

        // ----------------------------
        // TIME RANK
        // ----------------------------

        TimeRank = Rank.F;

        foreach (var frame in SelectedLevel.TimeRanks)
        {
            if (LevelEndTime <= frame.MaxTime)
            {
                TimeRank = frame.Rank;

                // Clamp to S max
                if (TimeRank == Rank.SS)
                    TimeRank = Rank.S;

                break;
            }
        }

        // If faster than best defined rank, clamp to S
        if (TimeRank == Rank.None)
            TimeRank = Rank.S;

        // ----------------------------
        // SCORE RANK
        // ----------------------------

        ScoreRank = Rank.F;

        foreach (var frame in SelectedLevel.ScoreRanks)
        {
            if (LevelEndScore >= frame.MinScore)
            {
                ScoreRank = frame.Rank;

                // Clamp to S max
                if (ScoreRank == Rank.SS)
                    ScoreRank = Rank.S;

                break;
            }
        }

        if (ScoreRank == Rank.None)
            ScoreRank = Rank.S;

        // ----------------------------
        // OVERALL RANK
        // ----------------------------

        int totalPoints =
            RankToPoints(KillCountRank) +
            RankToPoints(TimeRank) +
            RankToPoints(ScoreRank);

        Rank finalRank;

        if (totalPoints == 21)
        {
            finalRank = Rank.SS;
            bonusTickets = 5;
        }
        else if (totalPoints >= 20)
        {
            finalRank = Rank.S;
            bonusTickets = 3;
        }
        else if (totalPoints >= 17)
        {
            finalRank = Rank.A;
            bonusTickets = 1;
        }
        else if (totalPoints >= 14) finalRank = Rank.B;
        else if (totalPoints >= 11) finalRank = Rank.C;
        else if (totalPoints >= 8) finalRank = Rank.D;
        else if (totalPoints >= 5) finalRank = Rank.E;
        else finalRank = Rank.F;

        OverallRank = finalRank;

        // ----------------------------
        // SAVE BEST TIME + RANK
        // ----------------------------

        UpdateLevelRankings(LevelEndTime, finalRank);

        // ----------------------------
        // TICKETS
        // ----------------------------

        int finalTickets = CollectedTickets + bonusTickets;
        GivePlayerCollectedTickets(finalTickets);
    }

    private int RankToPoints(Rank rank)
    {
        switch (rank)
        {
            case Rank.SS: return 8;
            case Rank.S: return 7;
            case Rank.A: return 6;
            case Rank.B: return 5;
            case Rank.C: return 4;
            case Rank.D: return 3;
            case Rank.E: return 2;
            case Rank.F: return 1;
            default: return 0;
        }
    }

    public void UpdateLevelRankings(float levelCompletionTime, Rank levelOverallRanking)
    {
        var progress = LevelProgressManager.Instance.GetProgress(SelectedLevel);

        if (progress == null)
        {
            Debug.LogError("No progress data found for level.");
            return;
        }

        float newBestTime = progress.BestTime <= 0f
            ? levelCompletionTime
            : Mathf.Min(progress.BestTime, levelCompletionTime);

        Rank newBestRank = RankToPoints(levelOverallRanking) >
                           RankToPoints(progress.BestRank)
            ? levelOverallRanking
            : progress.BestRank;

        LevelProgressManager.Instance.UpdateProgress(
            SelectedLevel,
            newBestTime,
            newBestRank
        );
    }

    public void GivePlayerCollectedTickets(int AmountOfTickets)
    {
        PlayerInventoryManager.Instance.PlayerTickets += AmountOfTickets;
    }

    // --------------------
    // Helpers
    // --------------------

    private void ResetLevelStats()
    {
        LevelTime = 0f;
        Score = 0;
        EnemiesKilled = 0;
        CollectedTickets = 0;
        TimerRunning = false;

        KillCountRank = Rank.None;
        ScoreRank = Rank.None;
        TimeRank = Rank.None;
        OverallRank = Rank.None;
    }

    private void LevelEndCaptureStats()
    {
        LevelEndTime = LevelTime;
        LevelEndScore = Score;
        LevelEndKilCount = EnemiesKilled;
        LevelEndTickets = CollectedTickets;
    }

    private void OpenLevelEndScreen()
    {
        // TEMPORARY, will be a screen that shows values of killcount, time and score, and displays all ranks, but not yet
        ReturnToHub();
    }
}
