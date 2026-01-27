using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [Header("Boot Settings")]
    public string NextSceneName = "MainMenu";

    [Header("Managers Root")]
    public GameObject ManagersRoot;

    private void Awake()
    {
        if (ManagersRoot == null)
        {
            Debug.LogError("[BootLoader] ManagersRoot not assigned!");
            return;
        }

        DontDestroyOnLoad(ManagersRoot);
    }

    private void Start()
    {
        LoadSaveData();

        // Load next scene after all Awake() calls
        if (!string.IsNullOrEmpty(NextSceneName))
            SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
    }

    private void LoadSaveData()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
            Debug.Log("[BootLoader] Save loaded.");
        }
        else
        {
            Debug.LogWarning("[BootLoader] SaveManager not found.");
        }
    }
}
