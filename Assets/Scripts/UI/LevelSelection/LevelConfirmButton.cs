using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelConfirmButton : MonoBehaviour
{
    public void ConfirmSelectedLevel()
    {
        LevelManager.Instance.LoadSelectedLevel(LevelManager.Instance.SelectedLevel);
    }
}
