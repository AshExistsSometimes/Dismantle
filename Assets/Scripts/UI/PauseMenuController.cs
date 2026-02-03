using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header ("Scenes")]
    public string HubScene = "0.0_Hub";
    public string MainMenuScene = "MainMenu";
    [Header("Pause Screens")]
    public GameObject PauseScreen;
    public GameObject SettingsScreen;

    [Header("Level Only Buttons")]
    public GameObject HubButton;
    public GameObject CheckpointButton;
    public GameObject RestartButton;

    private string currentScene;
    private bool inHubScene;

    private UIManager uiManager;
    private SettingsManager settingsManager;

    [Header("Accent Colour")]
    public List<Image> menuElements = new List<Image>();
    public Color accentColour;


    private void Awake()
    {
        CurrentSceneCheck();
        UpdateAccentColour();
        PauseScreen.SetActive(true);
        SettingsScreen.SetActive(false);

        if (uiManager == null)
            uiManager = UIManager.Instance;
        if (settingsManager == null)
            settingsManager = SettingsManager.Instance;
    }

    private void OnEnable()
    {
        if (uiManager == null)
            uiManager = UIManager.Instance;
    }

    // // // // // // // // // // //
    public void ResumeGame()
    {
        uiManager.ClosePauseMenu();
        CloseSettings();
    }


    public void GoToLastCheckpoint()
    {
        // no checkpoints yet
        Debug.Log("NO CHECKPOINT SYSTEM YET");
    }



    public void RestartLevel()
    {
        currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(currentScene);
    }



    public void OpenSettings()
    {
        PauseScreen.SetActive(false);
        SettingsScreen.SetActive(true);
    }
    public void CloseSettings()
    {
        PauseScreen.SetActive(true);
        SettingsScreen.SetActive(false);
    }



    public void ReturnToHub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(HubScene);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
    }


    // Check Current Scene
    private void CurrentSceneCheck()
    {
        currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == HubScene)
        {
            inHubScene = true;
        }
        else { inHubScene = false; }


        HubButton.SetActive(!inHubScene);
        CheckpointButton.SetActive(!inHubScene);
        RestartButton.SetActive(!inHubScene);
    }

    public void UpdateAccentColour()
    {
        Debug.Log("Updating Colours of Pause Menu");

        if (settingsManager == null)
        {
            Debug.Log("No Settings Manager");
            settingsManager = SettingsManager.Instance;
        }

        accentColour = settingsManager.AccentColour;
        Debug.Log("Accent Colour = " + settingsManager.CurrentSettings.menuAccentColour);

        for (int i = 0; i < menuElements.Count; i++)
        {
            if (menuElements[i] != null)
            {
                menuElements[i].color = accentColour;
            }
        }
    }
}
