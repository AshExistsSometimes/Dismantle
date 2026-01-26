using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [Header("Boot Settings")]
    [Tooltip("Scene to load after boot is complete")]
    public string NextSceneName = "MainMenu";

    [Header("Managers")]
    [Tooltip("Root object that contains all managers")]
    public GameObject ManagersRoot;

    private void Awake()
    {
        Boot();
    }

    private void Boot()
    {
        // Persist managers only
        if (ManagersRoot != null)
        {
            DontDestroyOnLoad(ManagersRoot);
        }
        else
        {
            Debug.LogWarning("BootLoader: ManagersRoot not assigned.");
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(NextSceneName))
        {
            Debug.LogError("BootLoader: NextSceneName is empty!");
            return;
        }

        SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
    }
}
