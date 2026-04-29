using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public DialogueUI UI;

    private DialogueSO currentDialogue;
    private int currentIndex;

    public UnityEvent OnDialogueEnd;

    public bool InHub = false;

    private Coroutine autoPlayRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartDialogue(DialogueSO dialogue)
    {
        if (currentDialogue != null)
            return;

        if (dialogue == null || dialogue.Nodes.Count == 0)
            return;

        currentDialogue = dialogue;
        currentIndex = 0;

        UI.gameObject.SetActive(true);

        // Only halt player if specified
        if (dialogue.HaltPlayer)
        {
            if (!InHub)
            {
                PlayerWeaponManager.Instance.EquipmentEnabled = false;
            }

            GameManager.Instance.UIPauseGame();
        }

        UI.Open();
        ShowNode();
    }

    private void ShowNode()
    {
        if (currentIndex >= currentDialogue.Nodes.Count)
        {
            EndDialogue();
            return;
        }

        DialogueNodeSO node = currentDialogue.Nodes[currentIndex];

        // Display node with completion callback
        UI.DisplayNode(
            node,
            NextNode,
            OnNodeFinished,
            currentDialogue.HaltPlayer // true = manual, false = autoplay
        );
    }

    private void OnNodeFinished()
    {
        // Only auto-advance if NOT halting player
        if (currentDialogue == null || currentDialogue.HaltPlayer)
            return;

        if (autoPlayRoutine != null)
            StopCoroutine(autoPlayRoutine);

        autoPlayRoutine = StartCoroutine(AutoAdvanceAfterDelay());
    }

    private IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        NextNode();
    }

    private void NextNode()
    {
        currentIndex++;
        ShowNode();
    }

    private void EndDialogue()
    {
        if (autoPlayRoutine != null)
        {
            StopCoroutine(autoPlayRoutine);
            autoPlayRoutine = null;
        }

        UI.Close();
        UI.gameObject.SetActive(false);

        if (currentDialogue != null && currentDialogue.HaltPlayer)
        {
            GameManager.Instance.UIUnpauseGame();

            if (!InHub)
            {
                PlayerWeaponManager.Instance.EquipmentEnabled = true;
            }
        }

        OnDialogueEnd?.Invoke();
        OnDialogueEnd.RemoveAllListeners();

        currentDialogue = null;
    }
}