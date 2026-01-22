using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SaveFileName = "DismantleSave.sav";
    private string savePath;

    private Dictionary<string, ISaveable> saveables = new();
    private Dictionary<string, string> serializedData = new();

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
    }

    public void Register(string key, ISaveable saveable)
    {
        if (!saveables.ContainsKey(key))
            saveables.Add(key, saveable);
    }

    public void SaveGame()
    {
        serializedData.Clear();

        foreach (var pair in saveables)
        {
            string json = JsonUtility.ToJson(pair.Value.SaveState(), true);
            serializedData.Add(pair.Key, json);
        }

        string wrapperJson = JsonUtility.ToJson(new SaveWrapper(serializedData), true);
        File.WriteAllText(savePath, wrapperJson);

        Debug.Log("[SaveManager] Game saved");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("[SaveManager] No save file found");
            return;
        }

        string wrapperJson = File.ReadAllText(savePath);
        SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(wrapperJson);

        foreach (var pair in wrapper.data)
        {
            if (saveables.TryGetValue(pair.Key, out var saveable))
            {
                saveable.LoadState(JsonUtility.FromJson(pair.Value, saveable.SaveState().GetType()));
            }
        }

        Debug.Log("[SaveManager] Game loaded");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    [System.Serializable]
    private class SaveWrapper
    {
        public Dictionary<string, string> data;

        public SaveWrapper(Dictionary<string, string> data)
        {
            this.data = data;
        }
    }
}
