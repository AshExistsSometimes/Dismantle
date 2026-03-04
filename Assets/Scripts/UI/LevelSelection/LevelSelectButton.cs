using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class LevelSelectButton : MonoBehaviour
{
    public TMP_Text LevelName;
    public TMP_Text BestTime;
    public TMP_Text BestRank;

    public Image LevelIcon;

    public GameObject UnderDevelopmentOverlay;
    public GameObject LockedOverlay;

    public Button Button;

    public LevelSO level;
    [Space]
    public Image Background;
    public Color NormalColor;
    public Color SelectedColor;

    public static LevelSelectButton currentSelected;

    private Coroutine colorRoutine;
    public float ColorLerpSpeed = 8f;

    public void Setup(LevelSO levelSO)
    {
        level = levelSO;

        NormalColor = Background.color;

        LevelName.text = level.LevelName;

        if (LevelIcon != null)
            LevelIcon.sprite = level.LevelIcon;

        UnderDevelopmentOverlay.SetActive(false);
        LockedOverlay.SetActive(false);

        if (!level.IsImplemented)
        {
            UnderDevelopmentOverlay.SetActive(true);
            Button.interactable = false;
            BestTime.text = "--";
            BestRank.text = "-";
            return;
        }

        var progress = LevelProgressManager.Instance != null
            ? LevelProgressManager.Instance.GetProgress(level)
            : null;

        // If manager missing, treat as locked safely
        if (progress == null)
        {
            LockedOverlay.SetActive(true);
            Button.interactable = false;
            BestTime.text = "--";
            BestRank.text = "-";
            return;
        }

        if (!progress.Unlocked)
        {
            LockedOverlay.SetActive(true);
            Button.interactable = false;
            BestTime.text = "--";
            BestRank.text = "-";
            return;
        }

        // Implemented and Unlocked
        Button.interactable = true;

        if (!progress.Played)
        {
            BestTime.text = "--:--";
        }
        else
        {
            BestTime.text = FormatTime(progress.BestTime);
        }

        if (!progress.Played)
        {
            BestRank.text = " ";
        }
        else
        {
            BestRank.text = progress.BestRank.ToString();
        }

        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(Select);

        if (LevelManager.Instance != null &&
            LevelManager.Instance.SelectedLevel == level)
        {
            Select();
        }
        else
        {
            Deselect();
        }
    }

    public void Select()
    {
        if (level == null) return;

        // Deselect previous
        if (currentSelected != null && currentSelected != this)
            currentSelected.Deselect();

        currentSelected = this;

        LevelManager.Instance.SelectedLevel = level;

        StartColorLerp(SelectedColor);

        LevelSelectMenu menu = GetComponentInParent<LevelSelectMenu>();
        if (menu != null)
            menu.ScrollToButton(this);
    }

    public void Deselect()
    {
        StartColorLerp(NormalColor);
    }

    private string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);

        if (time.TotalHours >= 1f)
        {
            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
        }
        else
        {
            return $"{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
        }
    }




    private void StartColorLerp(Color target)
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = StartCoroutine(LerpColor(target));
    }

    private System.Collections.IEnumerator LerpColor(Color target)
    {
        Color start = Background.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * ColorLerpSpeed;
            Background.color = Color.Lerp(start, target, t);
            yield return null;
        }

        Background.color = target;
    }
}