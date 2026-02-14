using System.Collections.Generic;
using UnityEngine;

public class HighStreetTreadmill : MonoBehaviour
{
    [Header("References")]
    public Transform Player;
    public Transform StartingSegment;

    [Header("Prefabs")]
    public List<GameObject> HighStreetSegments = new List<GameObject>();

    [Header("Settings")]
    public int SegmentsPerSide = 2;
    public float SegmentLength = 200f;

    private Dictionary<int, GameObject> spawnedSegments = new Dictionary<int, GameObject>();
    private float startX;

    private int startingSegmentIndex;

    private Queue<GameObject> segmentPool = new Queue<GameObject>();
    private Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

    private void Start()
    {
        if (Player == null)
        {
            Debug.LogError("HighStreetTreadmill: Player reference missing.");
            enabled = false;
            return;
        }

        if (StartingSegment == null)
        {
            Debug.LogError("HighStreetTreadmill: StartingSegment reference missing.");
            enabled = false;
            return;
        }

        PrewarmPool();

        startX = StartingSegment.position.x;
        startingSegmentIndex = WorldXToSegmentIndex(startX);
        spawnedSegments[startingSegmentIndex] = StartingSegment.gameObject;

        int startIndex = WorldXToSegmentIndex(startX);
        spawnedSegments[startIndex] = StartingSegment.gameObject;
    }

    private void Update()
    {
        UpdateSegments();
    }

    private void UpdateSegments()
    {
        int playerSegmentIndex = WorldXToSegmentIndex(Player.position.x);

        int minIndex = playerSegmentIndex - SegmentsPerSide;
        int maxIndex = playerSegmentIndex + SegmentsPerSide;

        // Spawn needed segments
        for (int i = minIndex; i <= maxIndex; i++)
        {
            if (spawnedSegments.ContainsKey(i))
                continue;

            SpawnSegment(i);
        }

        // Despawn far segments
        List<int> toRemove = new List<int>();

        foreach (var kvp in spawnedSegments)
        {
            int index = kvp.Key;

            if (index == startingSegmentIndex)
                continue;

            if (index < minIndex || index > maxIndex)
            {
                toRemove.Add(index);
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            int index = toRemove[i];

            if (index == startingSegmentIndex)
                continue;

            GameObject instance = spawnedSegments[index];
            spawnedSegments.Remove(index);

            instance.SetActive(false);
            segmentPool.Enqueue(instance);
        }
    }

    private void SpawnSegment(int index)
    {
        if (segmentPool.Count == 0)
        {
            Debug.LogWarning("HighStreetTreadmill: Pool exhausted, instantiating extra segment.");
            GameObject fallback = Instantiate(HighStreetSegments[Random.Range(0, HighStreetSegments.Count)], transform);
            segmentPool.Enqueue(fallback);
            instanceToPrefab[fallback] = fallback;
        }

        GameObject instance = segmentPool.Dequeue();

        Vector3 position = StartingSegment.position;
        position.x = startX + (index * SegmentLength);

        instance.transform.SetPositionAndRotation(position, StartingSegment.rotation);
        instance.SetActive(true);

        spawnedSegments.Add(index, instance);
    }


    private int WorldXToSegmentIndex(float worldX)
    {
        return Mathf.FloorToInt((worldX - startX) / SegmentLength);
    }

    private int GetMinimumPoolSize()
    {
        int requiredUnique = Mathf.CeilToInt(((SegmentsPerSide * 2) + 1) / 2f);
        return Mathf.Max(requiredUnique, HighStreetSegments.Count);
    }

    private void PrewarmPool()
    {
        int minPoolSize = GetMinimumPoolSize();

        List<GameObject> spawnList = new List<GameObject>();

        while (spawnList.Count < minPoolSize)
        {
            for (int i = 0; i < HighStreetSegments.Count && spawnList.Count < minPoolSize; i++)
            {
                spawnList.Add(HighStreetSegments[i]);
            }
        }

        for (int i = 0; i < spawnList.Count; i++)
        {
            GameObject instance = Instantiate(spawnList[i], Vector3.one * 100000f, Quaternion.identity, transform);
            instance.SetActive(false);

            segmentPool.Enqueue(instance);
            instanceToPrefab[instance] = spawnList[i];
        }
    }

}

