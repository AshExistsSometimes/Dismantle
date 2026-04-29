using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject Root;
    public TMP_Text NameText;
    public TMP_Text DialogueText;
    public Image Portrait;
    public Image nextNodeImage;

    [Header("Buttons")]
    public Transform ButtonContainer;
    public GameObject ButtonPrefab;

    [Header("Typing")]
    public float CharactersPerSecond = 35f;
    public bool SkipOnClick = true;

    [Header("Audio")]
    public AudioSource VoiceSource;

    private Coroutine typingRoutine;
    private bool isTyping;
    private string fullText;

    private bool inputWasHeldDuringTyping;
    private bool canContinue;
    private bool awaitingRelease;
    private bool hasButtons;

    private float typingSpeedMultiplier = 1f;
    private float currentCharactersPerSecond;

    private System.Action onContinueCached;
    private System.Action onFinishedCached;

    private DialogueNodeSO currentNode;

    private bool allowManualAdvance = true;

    private void Awake()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.UI = this;
        }

        gameObject.SetActive(false);
    }

    private void Start()
    {
        DialogueText.richText = true;
    }

    private void Update()
    {
        if (!Root.activeSelf)
            return;

        // Typing input handling
        if (isTyping)
        {
            bool held = IsDialogueInputHeld();
            typingSpeedMultiplier = held ? 4f : 1f;

            if (held)
                inputWasHeldDuringTyping = true;

            return;
        }

        // Awaiting release after skipping
        if (awaitingRelease)
        {
            if (IsDialogueInputReleased())
                awaitingRelease = false;

            return;
        }

        if (canContinue && !hasButtons && allowManualAdvance && IsDialogueInputPressed())
        {
            canContinue = false;
            nextNodeImage.gameObject.SetActive(false);
            onContinueCached?.Invoke();
        }
    }

    public void Open()
    {
        Root.SetActive(true);
    }

    public void Close()
    {
        Root.SetActive(false);
        ClearButtons();
    }

    // UPDATED SIGNATURE
    public void DisplayNode(
        DialogueNodeSO node,
        System.Action onContinue,
        System.Action onFinished,
        bool allowInput
    )
    {
        currentNode = node;
        onContinueCached = onContinue;
        onFinishedCached = onFinished;
        allowManualAdvance = allowInput;

        inputWasHeldDuringTyping = false;

        // Per-node typing speed
        currentCharactersPerSecond = node.CharactersPerSecondOverride > 0
            ? node.CharactersPerSecondOverride
            : CharactersPerSecond;

        NameText.text = node.Speaker.CharacterName;
        Portrait.sprite = GetEmotionIcon(node);

        ClearButtons();

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        fullText = node.DialogueText;

        isTyping = false;
        canContinue = false;
        awaitingRelease = false;
        hasButtons = node.HasButtons;

        DialogueText.text = "";
        nextNodeImage.gameObject.SetActive(false);

        typingRoutine = StartCoroutine(TypeText(() =>
        {
            canContinue = true;
            awaitingRelease = inputWasHeldDuringTyping;

            if (!hasButtons && allowManualAdvance)
                nextNodeImage.gameObject.SetActive(true);

            currentNode = null;

            onFinishedCached?.Invoke();
        }));

        if (node.HasButtons)
        {
            nextNodeImage.gameObject.SetActive(false);

            foreach (var button in node.Buttons)
            {
                var btn = Instantiate(ButtonPrefab, ButtonContainer);
                btn.GetComponentInChildren<TMP_Text>().text = button.ButtonText;
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    button.ButtonEvent?.Invoke();
                });
            }
        }
    }

    private Sprite GetEmotionIcon(DialogueNodeSO node)
    {
        foreach (var emotion in node.Speaker.Emotions)
        {
            if (emotion.EmotionName == node.Emotion)
                return emotion.Icon;
        }
        return null;
    }

    private void ClearButtons()
    {
        foreach (Transform child in ButtonContainer)
            Destroy(child.gameObject);
    }

    private IEnumerator TypeText(System.Action onComplete)
    {
        isTyping = true;
        DialogueText.text = "";

        int i = 0;

        while (i < fullText.Length)
        {
            // Handle rich text instantly
            if (fullText[i] == '<')
            {
                int tagEnd = fullText.IndexOf('>', i);

                if (tagEnd != -1)
                {
                    DialogueText.text += fullText.Substring(i, tagEnd - i + 1);
                    i = tagEnd + 1;
                    continue;
                }
            }

            char c = fullText[i];
            DialogueText.text += c;

            if (!char.IsWhiteSpace(c) && currentNode?.Speaker?.VoiceBlip != null)
            {
                VoiceSource.pitch = Random.Range(
                    currentNode.Speaker.PitchRange.x,
                    currentNode.Speaker.PitchRange.y
                );
                VoiceSource.PlayOneShot(currentNode.Speaker.VoiceBlip);
            }

            float delay = 1f / (currentCharactersPerSecond * typingSpeedMultiplier);
            yield return new WaitForSeconds(delay);

            i++;
        }

        isTyping = false;
        onComplete?.Invoke();
    }

    public void SkipTyping()
    {
        if (!isTyping)
            return;

        StopCoroutine(typingRoutine);
        DialogueText.text = fullText;
        isTyping = false;

        canContinue = true;

        if (allowManualAdvance)
            nextNodeImage.gameObject.SetActive(true);

        onFinishedCached?.Invoke();
    }

    // -----------------
    // INPUT
    // -----------------
    private bool IsDialogueInputHeld()
    {
        return Input.GetKey(KeyCode.Space)
            || Input.GetKey(KeyCode.E)
            || Input.GetMouseButton(0);
    }

    private bool IsDialogueInputPressed()
    {
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.E)
            || Input.GetMouseButtonDown(0);
    }

    private bool IsDialogueInputReleased()
    {
        return Input.GetKeyUp(KeyCode.Space)
            || Input.GetKeyUp(KeyCode.E)
            || Input.GetMouseButtonUp(0);
    }
}