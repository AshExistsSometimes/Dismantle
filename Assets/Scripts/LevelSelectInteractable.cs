using UnityEngine;
using UnityEngine.UI;

public class LevelSelectInteractable : MonoBehaviour, IInteractable
{
    public LevelSelectMenu LevelSelectMenu;
    public Outline outline;

    private float defaultOutlineWidth;
    private Color defaultOutlineColour;

    private void Start()
    {
        // Cache Outline Data
        defaultOutlineColour = Color.cyan;
        defaultOutlineWidth = 0f;
    }

    public void OnHoverStart()
    {
        UIManager.Instance.ShowInteractText(true, "Press [E] to open Level Select");
        SetOutline(Color.white, 5f);
    }

    public void OnHoverStop()
    {
        SetOutline(defaultOutlineColour, defaultOutlineWidth);
        UIManager.Instance.ShowInteractText(false, null);
    }

    public void OnInteract()
    {
        LevelSelectMenu.gameObject.SetActive(true);
        LevelSelectMenu.OpenMenu();
    }

    public void SetOutline(Color color, float outlineWidth)
    {
        outline.OutlineColor = color;
        outline.OutlineWidth = outlineWidth;
    }
}
