using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCombatController : MonoBehaviour
{
    [Header("Doors")]
    public List<GameObject> CombatDoors = new List<GameObject>();

    [Header("Spawners")]
    public List<EnemySpawner> Spawnpoints = new List<EnemySpawner>();

    [Header("Waves")]
    public int WaveTracker = 0;
    public int TotalWaves = 3;

    private bool combatStarted = false;

    // Track enemies INSIDE this combat zone
    private List<BaseEnemy> enemiesInZone = new List<BaseEnemy>();

    private void Awake()
    {
        // Open doors
        foreach (var door in CombatDoors)
        {
            if (door != null)
                door.SetActive(false);
        }

        // Assign controller
        foreach (var spawner in Spawnpoints)
        {
            if (spawner != null)
                spawner.SetController(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Start combat
        if (!combatStarted && other.CompareTag("Player"))
        {
            StartCombat();
            return;
        }

        // Track enemies entering
        BaseEnemy enemy = other.GetComponent<BaseEnemy>();
        if (enemy != null && !enemiesInZone.Contains(enemy))
        {
            enemiesInZone.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BaseEnemy enemy = other.GetComponent<BaseEnemy>();
        if (enemy != null && enemiesInZone.Contains(enemy))
        {
            enemiesInZone.Remove(enemy);
        }
    }

    private void StartCombat()
    {
        combatStarted = true;

        foreach (var door in CombatDoors)
        {
            if (door != null)
                door.SetActive(true);
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        WaveTracker++;

        if (WaveTracker > TotalWaves)
        {
            EndCombat();
            return;
        }

        Debug.Log("Starting Wave: " + WaveTracker);

        foreach (var spawner in Spawnpoints)
        {
            spawner.SpawnWave(WaveTracker);
        }

        StartCoroutine(CheckWaveComplete());
    }

    private IEnumerator CheckWaveComplete()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.3f);

            int aliveCount = 0;

            for (int i = enemiesInZone.Count - 1; i >= 0; i--)
            {
                if (enemiesInZone[i] == null)
                {
                    enemiesInZone.RemoveAt(i);
                    continue;
                }

                if (!enemiesInZone[i].NoAI)
                    aliveCount++;
            }

            if (aliveCount == 0)
            {
                StartNextWave();
                yield break;
            }
        }
    }

    private void EndCombat()
    {
        Debug.Log("Combat Complete");

        foreach (var door in CombatDoors)
        {
            if (door != null)
                door.SetActive(false);
        }
    }
}