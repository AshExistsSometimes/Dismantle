using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public enum MenuState
    {
        Main,

        Settings,
        Settings_Audio,
        Settings_Video,
        Settings_UI,
        Settings_Gameplay,

        Extras,
        Extras_Gallery
    }

    [Header("Startup")]
    public Image universalBGImage;
    public float startupFadeDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private MenuState currentState = MenuState.Main;

    [Header("Audio")]
    public AudioClip ClickSound;
    public AudioClip BackSound;
    [Range(0f, 0.3f)]
    public float clickPitchVariance = 0.06f;
    private AudioSource audioSource;

    [Header("Accent Colour")]
    public Color MenuAccentColour = Color.red;
    public List<Image> menuElements = new List<Image>();

    [Header("Menu Roots")]
    public RectTransform mainMenuRoot;
    public RectTransform settingsMenuRoot;
    public RectTransform extrasMenuRoot;

    [Header("Root Slide Settings")]
    public float menuSlideDuration = 0.35f;
    public AnimationCurve menuSlideCurve;

    [Header("Tab Panel Slide Settings")]
    public float tabPanelSlideDuration = 0.35f;
    public AnimationCurve tabPanelSlideCurve;

    [Header("Tab Buttons")]
    public List<MenuButtonHover> settingsTabs;
    public List<MenuButtonHover> extrasTabs;

    [Header("Tab Panels")]
    public List<RectTransform> settingsMenus;
    public List<RectTransform> extrasMenus;

    [Header("Settings / Extras Background")]
    public RectTransform settingsExtrasBackground;

    public float backgroundSlideDuration = 0.4f;
    public AnimationCurve backgroundSlideCurve;

    [Header("Main Menu Background")]
    public RectTransform mainMenuBackground;
    public float mainMenuBackgroundSlideDuration = 0.4f;
    public AnimationCurve mainMenuBackgroundSlideCurve;

    [Header("Main Menu Sticker")]
    public RectTransform mainMenuSticker;
    public float stickerSlideDuration = 0.4f;
    public AnimationCurve stickerSlideCurve;


    private bool inputLocked;
    private Coroutine rootTransitionRoutine;

    private Vector2 stickerBasePosition;
    private Coroutine stickerRoutine;

    private MenuButtonHover currentTab;
    private RectTransform currentPanel;
    private int currentTabIndex = -1;

    private Dictionary<RectTransform, Vector2> panelBasePositions = new();

    private Vector2 backgroundBasePosition;
    private Coroutine backgroundRoutine;

    private Vector2 mainMenuBackgroundBasePosition;
    private Coroutine mainMenuBackgroundRoutine;

    ////////////////////////////////////////////////////////
    // Unity

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        CachePanelPositions(settingsMenus);
        CachePanelPositions(extrasMenus);

        mainMenuRoot.gameObject.SetActive(true);
        settingsMenuRoot.gameObject.SetActive(false);
        extrasMenuRoot.gameObject.SetActive(false);

        DeactivatePanels(settingsMenus);
        DeactivatePanels(extrasMenus);

        if (settingsExtrasBackground != null)
        {
            backgroundBasePosition = settingsExtrasBackground.anchoredPosition;

            // Start hidden offscreen to the right
            settingsExtrasBackground.anchoredPosition =
                backgroundBasePosition + Vector2.right * settingsExtrasBackground.rect.width;
        }

        if (SettingsManager.Instance != null)
        {
            MenuAccentColour =
                SettingsManager.Instance.CurrentSettings.menuAccentColour;

            ApplyAccentColour();
        }

        if (mainMenuSticker != null)
        {
            stickerBasePosition = mainMenuSticker.anchoredPosition;

            if (currentState != MenuState.Main)
            {
                mainMenuSticker.anchoredPosition =
                    stickerBasePosition + Vector2.right * mainMenuSticker.rect.width;
            }
        }

        if (mainMenuBackground != null)
        {
            mainMenuBackgroundBasePosition =
                mainMenuBackground.anchoredPosition;

            // If starting NOT on main, hide it below the screen
            if (currentState != MenuState.Main)
            {
                mainMenuBackground.anchoredPosition =
                    mainMenuBackgroundBasePosition
                    - Vector2.up * mainMenuBackground.rect.height;
            }
        }

        ForceInitialOffscreenState();
    }

    private void Start()
    {
        StartCoroutine(StartupSequence());
    }

    private void Update()
    {
        if (!inputLocked && Input.GetKeyDown(KeyCode.Escape))
            ReturnToMainMenu();
    }

    ////////////////////////////////////////////////////////
    // Button Entry Points

    public void OpenSettings()
    {
        PlayClick();
        ChangeRoot(MenuState.Settings);
        SelectDefaultTab(settingsTabs, settingsMenus, 0);
    }

    public void OpenExtras()
    {
        PlayClick();
        ChangeRoot(MenuState.Extras);
        SelectDefaultTab(extrasTabs, extrasMenus, 0);
    }

    public void QuitGame()
    {
        PlayClick();
        Application.Quit();
    }

    public void OpenSettingsAudio() { PlayClick(); OnTabClicked(settingsTabs, settingsMenus, 0); }
    public void OpenSettingsVideo() { PlayClick(); OnTabClicked(settingsTabs, settingsMenus, 1); }
    public void OpenSettingsUI() { PlayClick(); OnTabClicked(settingsTabs, settingsMenus, 2); }
    public void OpenSettingsGameplay() { PlayClick(); OnTabClicked(settingsTabs, settingsMenus, 3); }

    public void OpenExtrasGallery() { PlayClick(); OnTabClicked(extrasTabs, extrasMenus, 0); }

    public void PlayClick()
    {
        if (ClickSound == null) return;

        audioSource.pitch = 1f + Random.Range(-clickPitchVariance, clickPitchVariance);
        audioSource.PlayOneShot(ClickSound);
    }

    public void PlayBackSFX()
    {
        if (ClickSound == null) return;

        audioSource.pitch = 1f + Random.Range(-clickPitchVariance, clickPitchVariance);
        audioSource.PlayOneShot(BackSound);
    }


    ////////////////////////////////////////////////////////
    // Tab Logic

    private void OnTabClicked(List<MenuButtonHover> tabs, List<RectTransform> panels, int newIndex)
    {
        if (inputLocked || newIndex == currentTabIndex)
            return;

        int previousIndex = currentTabIndex;
        RectTransform previousPanel = currentPanel;

        if (currentTab != null)
            currentTab.SetLocked(false);

        currentTab = tabs[newIndex];
        currentTab.SetLocked(true);

        currentTabIndex = newIndex;
        currentPanel = panels[newIndex];

        if (previousPanel != null)
        {
            Vector2 exitDir = newIndex > previousIndex ? Vector2.up : Vector2.down;
            StartCoroutine(SlideOutAndDeactivate(previousPanel, exitDir));
        }

        Vector2 enterDir =
            previousIndex < 0 || newIndex > previousIndex
                ? Vector2.down
                : Vector2.up;

        ActivateAndSlideIn(currentPanel, enterDir);
    }

    private void SelectDefaultTab(List<MenuButtonHover> tabs, List<RectTransform> panels, int index)
    {
        currentTabIndex = -1;
        currentTab = null;
        currentPanel = null;

        OnTabClicked(tabs, panels, index);
    }

    ////////////////////////////////////////////////////////
    // Panel Animation

    private void ActivateAndSlideIn(RectTransform panel, Vector2 direction)
    {
        float offscreenDistance = GetPanelOffscreenDistance(panel);

        panel.gameObject.SetActive(true);
        panel.anchoredPosition =
            panelBasePositions[panel] + direction * offscreenDistance;

        StartCoroutine(SlidePanel(panel, panelBasePositions[panel]));
    }

    private IEnumerator SlideOutAndDeactivate(RectTransform panel, Vector2 direction)
    {
        float offscreenDistance = GetPanelOffscreenDistance(panel);

        yield return SlidePanel(
            panel,
            panel.anchoredPosition + direction * offscreenDistance
        );

        panel.gameObject.SetActive(false);
    }

    private IEnumerator SlidePanel(RectTransform panel, Vector2 target)
    {
        inputLocked = true;

        Vector2 start = panel.anchoredPosition;
        float time = 0f;

        while (time < tabPanelSlideDuration)
        {
            float t = time / tabPanelSlideDuration;
            float curveValue = tabPanelSlideCurve.Evaluate(t);

            panel.anchoredPosition =
                Vector2.LerpUnclamped(start, target, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = target;
        inputLocked = false;
    }

    private float GetPanelOffscreenDistance(RectTransform panel)
    {
        float panelHeight = panel.rect.height;
        float screenHeight = Screen.height;

        return Mathf.Max(panelHeight, screenHeight);
    }

    ////////////////////////////////////////////////////////
    // Root Transitions

    private void ChangeRoot(MenuState target)
    {
        if (inputLocked || currentState == target)
            return;

        RectTransform from = GetRoot(currentState);
        RectTransform to = GetRoot(target);

        bool wasMain = currentState == MenuState.Main;
        bool goingToMain = target == MenuState.Main;

        currentState = target;

        if (from != to)
            StartRootTransition(from, to);

        // Background logic
        if (wasMain && !goingToMain)
        {
            ShowSettingsExtrasBackground();
            HideMainMenuSticker();
            HideMainMenuBackground();
        }
        else if (!wasMain && goingToMain)
        {
            HideSettingsExtrasBackground();
            ShowMainMenuSticker();
            ShowMainMenuBackground();
        }
    }


    private void StartRootTransition(RectTransform from, RectTransform to)
    {
        if (rootTransitionRoutine != null)
            StopCoroutine(rootTransitionRoutine);

        rootTransitionRoutine = StartCoroutine(SlideRoots(from, to));
    }

    private IEnumerator SlideRoots(RectTransform from, RectTransform to)
    {
        inputLocked = true;

        Vector2 start = from.anchoredPosition;
        Vector2 end = start + Vector2.left * from.rect.width;

        Vector2 toStart = start + Vector2.left * to.rect.width;

        to.anchoredPosition = toStart;
        to.gameObject.SetActive(true);

        float time = 0f;
        while (time < menuSlideDuration)
        {
            float t = time / menuSlideDuration;
            float curveValue = menuSlideCurve.Evaluate(t);

            from.anchoredPosition = Vector2.LerpUnclamped(start, end, curveValue);
            to.anchoredPosition = Vector2.LerpUnclamped(toStart, start, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        from.gameObject.SetActive(false);
        from.anchoredPosition = start;
        to.anchoredPosition = start;

        inputLocked = false;
    }

    private RectTransform GetRoot(MenuState state)
    {
        if (state.ToString().StartsWith("Settings"))
            return settingsMenuRoot;

        if (state.ToString().StartsWith("Extras"))
            return extrasMenuRoot;

        return mainMenuRoot;
    }

    ////////////////////////////////////////////////////////
    // Back

    private void ReturnToMainMenu()
    {
        if (inputLocked || currentState == MenuState.Main)
            return;

        PlayBackSFX();

        ChangeRoot(MenuState.Main);

        if (currentTab != null)
            currentTab.SetLocked(false);

        if (currentPanel != null)
            currentPanel.gameObject.SetActive(false);

        currentTab = null;
        currentPanel = null;
        currentTabIndex = -1;
    }

    ////////////////////////////////////////////////////////
    // Helpers

    private void CachePanelPositions(List<RectTransform> panels)
    {
        foreach (var p in panels)
            if (p != null)
                panelBasePositions[p] = p.anchoredPosition;
    }

    private void DeactivatePanels(List<RectTransform> panels)
    {
        foreach (var p in panels)
            if (p != null)
                p.gameObject.SetActive(false);
    }

    // Colour Control //////////////////////////////////////

    [ContextMenu("Apply Accent Colour")]
    public void ApplyAccentColour()
    {
        for (int i = 0; i < menuElements.Count; i++)
        {
            if (menuElements[i] != null)
            {
                menuElements[i].color = MenuAccentColour;
            }
        }

        ConfirmAccentColour(MenuAccentColour);
    }

    public void ConfirmAccentColour(Color newColour)
    {
        SettingsManager.Instance.SetMenuAccentColour(newColour);
    }

    ////////////////////////////////////////////////////////

    // Level Loading ///////////////////////////////////////

    public void LoadLevel(int sceneIndex)
    {
        if (inputLocked)
            return;

        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!loadOperation.isDone)
        {
            Debug.Log(loadOperation.progress);
            yield return null;
        }
    }

    private void ShowSettingsExtrasBackground()
    {
        if (settingsExtrasBackground == null)
            return;

        if (backgroundRoutine != null)
            StopCoroutine(backgroundRoutine);

        float width = settingsExtrasBackground.rect.width;

        settingsExtrasBackground.anchoredPosition =
            backgroundBasePosition + Vector2.right * width;

        backgroundRoutine = StartCoroutine(
            SlideBackground(settingsExtrasBackground, backgroundBasePosition)
        );
    }

    private void HideSettingsExtrasBackground()
    {
        if (settingsExtrasBackground == null)
            return;

        if (backgroundRoutine != null)
            StopCoroutine(backgroundRoutine);

        float width = settingsExtrasBackground.rect.width;

        Vector2 target =
            backgroundBasePosition + Vector2.right * width;

        backgroundRoutine = StartCoroutine(
            SlideBackground(settingsExtrasBackground, target)
        );
    }

    private IEnumerator SlideBackground(RectTransform bg, Vector2 target)
    {
        Vector2 start = bg.anchoredPosition;
        float time = 0f;

        while (time < backgroundSlideDuration)
        {
            float t = time / backgroundSlideDuration;
            float curveValue = backgroundSlideCurve.Evaluate(t);

            bg.anchoredPosition =
                Vector2.LerpUnclamped(start, target, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        bg.anchoredPosition = target;
    }

    private void ShowMainMenuSticker()
    {
        if (mainMenuSticker == null)
            return;

        if (stickerRoutine != null)
            StopCoroutine(stickerRoutine);

        float width = mainMenuSticker.rect.width;

        mainMenuSticker.anchoredPosition =
            stickerBasePosition + Vector2.right * width;

        stickerRoutine = StartCoroutine(
            SlideSticker(mainMenuSticker, stickerBasePosition)
        );
    }

    private void HideMainMenuSticker()
    {
        if (mainMenuSticker == null)
            return;

        if (stickerRoutine != null)
            StopCoroutine(stickerRoutine);

        float width = mainMenuSticker.rect.width;

        Vector2 target =
            stickerBasePosition + Vector2.right * width;

        stickerRoutine = StartCoroutine(
            SlideSticker(mainMenuSticker, target)
        );
    }

    private IEnumerator SlideSticker(RectTransform sticker, Vector2 target)
    {
        Vector2 start = sticker.anchoredPosition;
        float time = 0f;

        while (time < stickerSlideDuration)
        {
            float t = time / stickerSlideDuration;
            float curveValue = stickerSlideCurve.Evaluate(t);

            sticker.anchoredPosition =
                Vector2.LerpUnclamped(start, target, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        sticker.anchoredPosition = target;
    }

    private void ShowMainMenuBackground()
    {
        if (mainMenuBackground == null)
            return;

        if (mainMenuBackgroundRoutine != null)
            StopCoroutine(mainMenuBackgroundRoutine);

        float height = mainMenuBackground.rect.height;

        mainMenuBackground.anchoredPosition =
            mainMenuBackgroundBasePosition - Vector2.up * height;

        mainMenuBackgroundRoutine = StartCoroutine(
            SlideMainMenuBackground(mainMenuBackgroundBasePosition)
        );
    }

    private void HideMainMenuBackground()
    {
        if (mainMenuBackground == null)
            return;

        if (mainMenuBackgroundRoutine != null)
            StopCoroutine(mainMenuBackgroundRoutine);

        float height = mainMenuBackground.rect.height;

        Vector2 target =
            mainMenuBackgroundBasePosition - Vector2.up * height;

        mainMenuBackgroundRoutine = StartCoroutine(
            SlideMainMenuBackground(target)
        );
    }

    private IEnumerator SlideMainMenuBackground(Vector2 target)
    {
        Vector2 start = mainMenuBackground.anchoredPosition;
        float time = 0f;

        while (time < mainMenuBackgroundSlideDuration)
        {
            float t = time / mainMenuBackgroundSlideDuration;
            float curveValue = mainMenuBackgroundSlideCurve.Evaluate(t);

            mainMenuBackground.anchoredPosition =
                Vector2.LerpUnclamped(start, target, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        mainMenuBackground.anchoredPosition = target;
    }

    private void ForceInitialOffscreenState()
    {
        // Main root (as if coming FROM settings)
        mainMenuRoot.anchoredPosition +=
            Vector2.left * mainMenuRoot.rect.width;

        // Settings / Extras background (right)
        if (settingsExtrasBackground != null)
        {
            settingsExtrasBackground.anchoredPosition =
                backgroundBasePosition +
                Vector2.right * settingsExtrasBackground.rect.width;
        }

        // Main menu sticker (right)
        if (mainMenuSticker != null)
        {
            mainMenuSticker.anchoredPosition =
                stickerBasePosition +
                Vector2.right * mainMenuSticker.rect.width;
        }

        // Main menu background (down)
        if (mainMenuBackground != null)
        {
            mainMenuBackground.anchoredPosition =
                mainMenuBackgroundBasePosition -
                Vector2.up * mainMenuBackground.rect.height;
        }

        // Universal BG invisible
        if (universalBGImage != null)
        {
            Color c = universalBGImage.color;
            c.a = 0f;
            universalBGImage.color = c;
        }
    }

    private IEnumerator StartupSequence()
    {
        inputLocked = true;

        // Fade in universal background
        if (universalBGImage != null)
        {
            Color c = universalBGImage.color;
            float time = 0f;

            while (time < startupFadeDuration)
            {
                c.a = Mathf.Lerp(0f, 1f, time / startupFadeDuration);
                universalBGImage.color = c;

                time += Time.deltaTime;
                yield return null;
            }

            c.a = 1f;
            universalBGImage.color = c;
        }

        // Slide main menu root in (reuse root logic)
        StartRootTransition(
            settingsMenuRoot,
            mainMenuRoot
        );

        // Slide visuals
        ShowMainMenuSticker();
        ShowMainMenuBackground();

        inputLocked = false;
    }

}
