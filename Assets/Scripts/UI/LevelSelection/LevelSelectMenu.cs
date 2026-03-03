using System.Collections.Generic;
using UnityEngine;

public class LevelSelectMenu : MonoBehaviour
{
    [Header("Data")]
    public List<BountySO> Bounties = new();

    [Header("UI")]
    public Transform BountyListParent;
    public BountyBoxUI BountyBoxPrefab;

    private void OnEnable()
    {
        BuildMenu();
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
        gameObject.SetActive(false);
        GameManager.Instance.UIUnpauseGame();
    }

    public void OpenMenu()
    {
        GameManager.Instance.UIPauseGame();
        gameObject.SetActive(true);
    }
}