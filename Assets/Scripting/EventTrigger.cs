using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    public UnityEvent EnterTriggerEvent;
    public UnityEvent ExitTriggerEvent;

    public bool TriggerOnce = false;

    private bool CanTrigger = true;
    private bool ExitCanTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CanTrigger)
        {
            EnterTriggerEvent.Invoke();

            if (TriggerOnce)
            {
                CanTrigger = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && ExitCanTrigger)
        {
            ExitTriggerEvent.Invoke();

            if (TriggerOnce)
            {
                ExitCanTrigger = false;
            }
        }
    }
}
