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

    [Header("State")]
    public bool TimerRunning { get; private set; }

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

    public void AddTicket()
    {
        CollectedTickets++;
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
    }
}
