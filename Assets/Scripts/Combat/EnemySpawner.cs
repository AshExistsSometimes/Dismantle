using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject EnemyPrefab;
        public int Wave;
    }

    public List<SpawnEntry> Enemies = new List<SpawnEntry>();

    [Header("Spawn Settings")]
    public float SpawnRadius = 3f;

    private WaveCombatController controller;
    private HashSet<SpawnEntry> spawnedEntries = new HashSet<SpawnEntry>();

    public void SetController(WaveCombatController c)
    {
        controller = c;
    }

    public void SpawnWave(int wave)
    {
        List<SpawnEntry> toSpawn = new List<SpawnEntry>();

        foreach (var entry in Enemies)
        {
            if (entry.Wave == wave && !spawnedEntries.Contains(entry))
            {
                toSpawn.Add(entry);
                spawnedEntries.Add(entry);
            }
        }

        if (toSpawn.Count == 0) return;

        if (toSpawn.Count == 1)
        {
            SpawnEnemy(toSpawn[0].EnemyPrefab, transform.position);
        }
        else
        {
            foreach (var entry in toSpawn)
            {
                Vector3 offset = Random.insideUnitSphere * SpawnRadius;
                offset.y = 0f;

                SpawnEnemy(entry.EnemyPrefab, transform.position + offset);
            }
        }
    }

    private void SpawnEnemy(GameObject prefab, Vector3 pos)
    {
        Instantiate(prefab, pos, Quaternion.identity);
    }

    // --------------------
    // GIZMOS
    // --------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, SpawnRadius);
    }
}