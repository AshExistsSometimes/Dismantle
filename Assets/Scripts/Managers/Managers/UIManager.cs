using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    public GameManager gameManager;
    public GameObject PauseMenuPrefab;

    [Space]

    public TMP_Text InteractText;
    public string InteractionText = "Press [E] to Interact";
    private string defaultInteractionText;

    [Header("Pause Control")]
    public bool CanPause = true;

    private GameObject pauseMenuInstance;

    // --------------------
    // Unity
    // --------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gameManager == null)
            gameManager = GameManager.Instance;

        EvaluateScenePauseState();
        SceneManager.sceneLoaded += OnSceneLoaded;

        defaultInteractionText = InteractionText;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!CanPause)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            HandlePauseToggle();
    }

    // --------------------
    // Scene Handling
    // --------------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateScenePauseState();
    }

    private void EvaluateScenePauseState()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Boot" || sceneName == "MainMenu")
        {
            DisablePause();
        }
        else
        {
            AllowPause();
        }
    }

    // --------------------
    // Pause Control
    // --------------------

    public void AllowPause()
    {
        CanPause = true;
    }

    public void DisablePause()
    {
        CanPause = false;

        // Force unpause if pause gets disabled mid-pause (cutscenes, etc.)
        if (gameManager.IsPaused)
            ClosePauseMenu();
    }

    private void HandlePauseToggle()
    {
        // UI already open but not paused
        if (!gameManager.IsPaused && gameManager.UIOpen)
            return;

        if (!gameManager.IsPaused)
            OpenPauseMenu();
        else
            ClosePauseMenu();
    }

    private void OpenPauseMenu()
    {
        if (!CanPause)
            return;

        if (pauseMenuInstance == null)
        {
            pauseMenuInstance = Instantiate(PauseMenuPrefab);
        }
        else
        {
            pauseMenuInstance.SetActive(true);
        }

        gameManager.IsPaused = true;
        gameManager.UIOpen = true;
        Time.timeScale = 0f;
    }

    private void ClosePauseMenu()
    {
        if (pauseMenuInstance != null)
            pauseMenuInstance.SetActive(false);

        gameManager.IsPaused = false;
        gameManager.UIOpen = false;
        Time.timeScale = 1f;
    }

    public void ShowInteractText(bool show, string text)
    {
        if (InteractText != null)
            InteractText.gameObject.SetActive(show);

        if (text == null)
        {
            InteractText.text = defaultInteractionText;
        }
        else
        {
            InteractText.text = text;
        }
    }
}
