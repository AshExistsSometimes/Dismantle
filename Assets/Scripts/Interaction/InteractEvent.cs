using UnityEngine;
using UnityEngine.Events;

public class InteractEvent : MonoBehaviour, IInteractable
{
    private float defaultOutlineWidth;
    private Color defaultOutlineColour;
    public Color HoverOutlineColour = Color.white;

    public Outline outline;

    public string InteractText = "to Pick Up";

    public UnityEvent WhenInteracted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultOutlineColour = Color.black;
        defaultOutlineWidth = 5;
    }

    public void OnInteract()
    {
        WhenInteracted.Invoke();
    }

    public void OnHoverStart()
    {
        SetOutline(HoverOutlineColour, 5f);
        UIManager.Instance.ShowInteractText(true, InteractText);
    }

    public void OnHoverStop()
    {
        SetOutline(defaultOutlineColour, defaultOutlineWidth);
        UIManager.Instance.ShowInteractText(false, null);
    }

    public void SetOutline(Color color, float outlineWidth)
    {
        outline.OutlineColor = color;
        outline.OutlineWidth = outlineWidth;
    }
}
