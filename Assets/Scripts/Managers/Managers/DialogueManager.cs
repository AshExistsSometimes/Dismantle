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

        UI.gameObject.SetActive(true);
        GameManager.Instance.EnterDialogue();

        currentDialogue = dialogue;
        currentIndex = 0;

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
        UI.DisplayNode(node, NextNode);
    }

    private void NextNode()
    {
        currentIndex++;
        ShowNode();
    }

    private void EndDialogue()
    {
        UI.Close();
        UI.gameObject.SetActive(false);

        GameManager.Instance.ExitDialogue();

        OnDialogueEnd?.Invoke();
        OnDialogueEnd.RemoveAllListeners();

        currentDialogue = null;
    }


}

