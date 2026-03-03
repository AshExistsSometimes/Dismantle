using UnityEngine;
using TMPro;

public class BountyBoxUI : MonoBehaviour
{
    public TMP_Text BountyTitle;
    public Transform LevelsParent;
    public LevelSelectButton LevelButtonPrefab;

    public void Setup(BountySO bounty)
    {
        BountyTitle.text = bounty.BountyName;

        foreach (Transform child in LevelsParent)
            Destroy(child.gameObject);

        foreach (var level in bounty.Levels)
        {
            var button = Instantiate(LevelButtonPrefab, LevelsParent);
            button.Setup(level);
        }
    }
}
