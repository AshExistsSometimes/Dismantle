using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueSO Dialogue;

    public bool CanTrigger = true;

    public bool TriggerOnce = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!CanTrigger) return;

        if (TriggerOnce)
        {
            CanTrigger = false;
            gameObject.SetActive(false);
        }

        DialogueManager.Instance.StartDialogue(Dialogue);
    }
}
