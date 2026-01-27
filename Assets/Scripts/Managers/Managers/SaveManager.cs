using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SaveFileName = "DismantleSave.sav";
    private string savePath;
    public string ReadOnlySavePath;

    private readonly Dictionary<string, ISaveable> saveables = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, SaveFileName);

        Debug.Log($"[SaveManager] Save path: {savePath}");

        ReadOnlySavePath = savePath;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveGame();
    }

    public void Register(ISaveable saveable)
    {
        if (!saveables.ContainsKey(saveable.SaveKey))
            saveables.Add(saveable.SaveKey, saveable);
    }

    public void Unregister(ISaveable saveable)
    {
        if (saveables.ContainsKey(saveable.SaveKey))
            saveables.Remove(saveable.SaveKey);
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        StringBuilder builder = new StringBuilder();

        foreach (var saveable in saveables.Values)
        {
            builder.AppendLine($"{saveable.SaveKey}:");

            var data = saveable.CaptureSaveData();
            foreach (var pair in data)
                builder.AppendLine($"{pair.Key} - {pair.Value}");

            builder.AppendLine();
        }

        File.WriteAllText(savePath, builder.ToString());
        Debug.Log("[SaveManager] Game saved");
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[SaveManager] No save file found");
            return;
        }

        string[] lines = File.ReadAllLines(savePath);

        ISaveable current = null;
        Dictionary<string, string> buffer = new();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (current != null)
                {
                    current.RestoreSaveData(buffer);
                    buffer.Clear();
                    current = null;
                }
                continue;
            }

            if (line.EndsWith(":"))
            {
                string key = line.Replace(":", "");
                saveables.TryGetValue(key, out current);
                continue;
            }

            if (current == null) continue;

            int splitIndex = line.IndexOf(" - ");
            if (splitIndex <= 0) continue;

            string dataKey = line[..splitIndex];
            string value = line[(splitIndex + 3)..];

            buffer[dataKey] = value;
        }

        if (current != null)
            current.RestoreSaveData(buffer);

        Debug.Log("[SaveManager] Game loaded");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}