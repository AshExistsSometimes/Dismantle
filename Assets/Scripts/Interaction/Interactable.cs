using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public KeyCode InteractKey = KeyCode.E;

    private IInteractable target;
    private bool isHovering;

    private void Awake()
    {
        target = GetComponent<IInteractable>();
        if (target == null)
            Debug.LogError($"{name} needs IInteractable");
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        target?.OnHoverStart();
    }

    private void OnMouseExit()
    {
        isHovering = false;
        target?.OnHoverStop();
    }

    private void Update()
    {
        if (!isHovering)
            return;

        if (Input.GetKeyDown(InteractKey))
            target?.OnInteract();
    }
}
