using UnityEngine;
using UnityEngine.SceneManagement;

public class DEBUG_SceneLoader : MonoBehaviour, IInteractable
{
    public string SceneName = "Scene Name";

    private float defaultOutlineWidth;
    private Color defaultOutlineColour;
    public Color HoverOutlineColour = Color.white;

    public Outline outline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultOutlineColour = Color.black;
        defaultOutlineWidth = 5;
    }


    public void OnHoverStart()
    {
        Debug.Log ("Player Looking at:" + name );
        UIManager.Instance.ShowInteractText(true, "Press [E] to load scene:" + SceneName);
        SetOutline(HoverOutlineColour, 5f);
    }

    public void OnHoverStop()
    {
        SetOutline(defaultOutlineColour, defaultOutlineWidth);
        UIManager.Instance.ShowInteractText(false, null);
    }

    public void OnInteract()
    {
        SceneManager.LoadScene(SceneName);
    }

    public void SetOutline(Color color, float outlineWidth)
    {
        outline.OutlineColor = color;
        outline.OutlineWidth = outlineWidth;
    }
}
