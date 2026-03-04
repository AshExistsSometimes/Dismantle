using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelEndTrigger : MonoBehaviour
{
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (!other.CompareTag("Player")) return;

        triggered = true;

        LevelManager.Instance.LevelComplete();
        DialogueManager.Instance.InHub = true;
    }
}
