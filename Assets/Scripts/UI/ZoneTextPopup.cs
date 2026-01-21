using UnityEngine;
using TMPro;
using System.Collections;

public class ZoneTextPopup : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text ZoneText;
    public RectTransform TopVignette;
    public RectTransform BottomVignette;

    [Header("Text Timing")]
    public float DisplayTime = 2f;
    public float FadeInTime = 0.5f;
    public float FadeOutTime = 0.5f;
    public float TextStartDelay = 0.1f;

    [Header("Typewriter")]
    public bool UseTypewriterEffect = false;
    public float TypewriterCharactersPerSecond = 30f;

    [Header("Vignette Movement")]
    public float ScrollTime = 0.5f;
    public float AnimationStartOffset = 200f;

    [Header("Easing Curves")]
    public AnimationCurve TextFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve VignetteMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Appearance")]
    public Color DisplayColour = Color.white;

    private Vector2 topVignetteOriginalPos;
    private Vector2 bottomVignetteOriginalPos;

    private Coroutine popupRoutine;

    private void Awake()
    {
        topVignetteOriginalPos = TopVignette.anchoredPosition;
        bottomVignetteOriginalPos = BottomVignette.anchoredPosition;

        ZoneText.gameObject.SetActive(false);
        TopVignette.gameObject.SetActive(false);
        BottomVignette.gameObject.SetActive(false);
    }

    public void ShowDisplayPopup(string displayText)
    {
        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(PopupSequence(displayText));
    }

    private IEnumerator PopupSequence(string displayText)
    {
        ResetVignettePositions();

        TopVignette.gameObject.SetActive(true);
        BottomVignette.gameObject.SetActive(true);

        ZoneText.text = string.Empty;
        ZoneText.color = new Color(
            DisplayColour.r,
            DisplayColour.g,
            DisplayColour.b,
            UseTypewriterEffect ? 1f : 0f
        );

        ZoneText.gameObject.SetActive(true);

        // Start vignette movement immediately
        StartCoroutine(ScrollVignettes(true));

        // Delay before text begins
        yield return new WaitForSeconds(TextStartDelay);

        if (UseTypewriterEffect)
        {
            yield return StartCoroutine(Typewriter(displayText));
        }
        else
        {
            ZoneText.text = displayText;
            yield return StartCoroutine(FadeText(0f, 1f, FadeInTime));
        }

        yield return new WaitForSeconds(DisplayTime);

        yield return StartCoroutine(FadeText(1f, 0f, FadeOutTime));
        yield return StartCoroutine(ScrollVignettes(false));

        ZoneText.gameObject.SetActive(false);
        TopVignette.gameObject.SetActive(false);
        BottomVignette.gameObject.SetActive(false);
    }

    private IEnumerator FadeText(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = TextFadeCurve.Evaluate(elapsed / duration);

            float alpha = Mathf.Lerp(from, to, t);
            ZoneText.color = new Color(
                DisplayColour.r,
                DisplayColour.g,
                DisplayColour.b,
                alpha
            );

            yield return null;
        }
    }

    private IEnumerator ScrollVignettes(bool moveIn)
    {
        float elapsed = 0f;

        Vector2 topStart = moveIn
            ? topVignetteOriginalPos + Vector2.up * AnimationStartOffset
            : topVignetteOriginalPos;

        Vector2 topEnd = moveIn
            ? topVignetteOriginalPos
            : topVignetteOriginalPos + Vector2.up * AnimationStartOffset;

        Vector2 bottomStart = moveIn
            ? bottomVignetteOriginalPos + Vector2.down * AnimationStartOffset
            : bottomVignetteOriginalPos;

        Vector2 bottomEnd = moveIn
            ? bottomVignetteOriginalPos
            : bottomVignetteOriginalPos + Vector2.down * AnimationStartOffset;

        while (elapsed < ScrollTime)
        {
            elapsed += Time.deltaTime;
            float t = VignetteMoveCurve.Evaluate(elapsed / ScrollTime);

            TopVignette.anchoredPosition = Vector2.Lerp(topStart, topEnd, t);
            BottomVignette.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, t);

            yield return null;
        }
    }

    private IEnumerator Typewriter(string fullText)
    {
        ZoneText.text = string.Empty;

        float delay = 1f / Mathf.Max(TypewriterCharactersPerSecond, 1f);

        for (int i = 0; i < fullText.Length; i++)
        {
            ZoneText.text += fullText[i];
            yield return new WaitForSeconds(delay);
        }
    }

    private void ResetVignettePositions()
    {
        TopVignette.anchoredPosition = topVignetteOriginalPos + Vector2.up * AnimationStartOffset;
        BottomVignette.anchoredPosition = bottomVignetteOriginalPos + Vector2.down * AnimationStartOffset;
    }
}
