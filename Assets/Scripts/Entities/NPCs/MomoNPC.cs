using UnityEngine;

public class MomoNPC : BaseNPC, IInteractable
{
    private float defaultOutlineWidth;
    private Color defaultOutlineColour;

    [Header("Dialogue")]
    public DialogueSO Dialogue;

    // Unity Default //
    private void Start()
    {
        // Cache Outline Data
        defaultOutlineColour = Color.black;
        defaultOutlineWidth = MaxOutlineThickness;
    }

    // Interaction // 
    public void OnHoverStart()
    {
        Debug.Log("[Momo] The Player is looking at me");
        canUpdateOutline = false;
        UIManager.Instance.ShowInteractText(true, "Press [E] to talk to Momo");
        SetOutline(Color.white, 5f);
    }

    public void OnHoverStop()
    {
        Debug.Log("[Momo] The Player has stopped looking at me");
        SetOutline(defaultOutlineColour, defaultOutlineWidth);
        UIManager.Instance.ShowInteractText(false, null);
        canUpdateOutline = true;
    }

    public void OnInteract()
    {
        Debug.Log("[Momo] The Player is talking to me");
        DialogueManager.Instance.StartDialogue(Dialogue);
    }


    public void SetOutline(Color color, float outlineWidth)
    {
        outline.OutlineColor = color;
        outline.OutlineWidth = outlineWidth;
    }
}
