using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    private IInteractable currentHover;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance.UIOpen)
        {
            ClearHover();
            return;
        }

        CheckHover();

        if (currentHover != null && Input.GetKeyDown(KeyCode.E))
        {
            currentHover.OnInteract();
        }
    }

    private void CheckHover()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable != currentHover)
            {
                ClearHover();
                currentHover = interactable;
                currentHover.OnHoverStart();

                UIManager.Instance?.ShowInteractText(true, null);
            }

            return;
        }

        ClearHover();
    }

    private void ClearHover()
    {
        if (currentHover != null)
        {
            currentHover.OnHoverStop();
            currentHover = null;

            UIManager.Instance?.ShowInteractText(false, null);
        }
    }
}

