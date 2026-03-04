using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour
{
    [Header("Data")]
    public List<BountySO> Bounties = new();

    [Header("UI")]
    public Transform BountyListParent;
    public BountyBoxUI BountyBoxPrefab;
    public ScrollRect scrollRect;

    public GameObject PlayerUI;

    private bool LevelSelectOpen = false;

    private void OnEnable()
    {
        BuildMenu();
    }

    private void Update()
    {
        if (!LevelSelectOpen)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && LevelSelectOpen)
        {
            CloseMenu();
        }
    }

    public void BuildMenu()
    {
        Debug.Log("Building Menu. Bounty count: " + Bounties.Count);

        foreach (Transform child in BountyListParent)
            Destroy(child.gameObject);

        foreach (var bounty in Bounties)
        {
            Debug.Log("Spawning bounty: " + bounty.BountyName);

            var bountyUI = Instantiate(BountyBoxPrefab, BountyListParent);
            bountyUI.Setup(bounty);
        }
    }

    public void ConfirmLevel()
    {
        if (LevelManager.Instance.SelectedLevel == null)
        {
            Debug.LogWarning("No level selected!");
            return;
        }

        LevelManager.Instance.LoadSelectedLevel(LevelManager.Instance.SelectedLevel);
    }

    public void CloseMenu()
    {
        LevelSelectOpen = false;
        gameObject.SetActive(false);
        GameManager.Instance.UIUnpauseGame();
        PlayerUI.SetActive(true);
        UIManager.Instance.CanPause = true;
    }

    public void OpenMenu()
    {
        LevelSelectOpen = true;
        GameManager.Instance.UIPauseGame();
        UIManager.Instance.CanPause = false;
        PlayerUI.SetActive(false);
        gameObject.SetActive(true);
    }

    public void ScrollToButton(LevelSelectButton button)
    {
        Canvas.ForceUpdateCanvases();

        RectTransform content = scrollRect.content;
        RectTransform target = button.GetComponent<RectTransform>();

        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        float targetY = Mathf.Abs(target.anchoredPosition.y);

        float normalized = Mathf.Clamp01(targetY / (contentHeight - viewportHeight));

        scrollRect.verticalNormalizedPosition = 1f - normalized;
    }
}