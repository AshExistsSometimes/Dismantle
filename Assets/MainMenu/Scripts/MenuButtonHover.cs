using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Audio")]// Audio logic will be removed later for a sound manager
    public AudioClip SwipeSound;
    [Range(0f, 0.3f)]
    public float pitchVariance = 0.05f;

    private AudioSource audioSource;

    [Header("Tab State (Optional)")]
    public MainMenuManager.MenuState tabState;

    [Header("Hover Settings")]
    public float hoverOffset = 20f;
    public float hoverDuration = 0.25f;
    public AnimationCurve hoverCurve;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine hoverRoutine;

    public bool locked;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (locked) return;

        PlaySwipe();

        StartHover(originalPosition + Vector2.right * hoverOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (locked) return;
        StartHover(originalPosition);
    }

    private void StartHover(Vector2 targetPosition)
    {
        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = StartCoroutine(HoverAnimation(targetPosition));
    }

    private IEnumerator HoverAnimation(Vector2 target)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float time = 0f;

        while (time < hoverDuration)
        {
            float t = time / hoverDuration;
            float curveValue = hoverCurve.Evaluate(t);

            rectTransform.anchoredPosition =
                Vector2.LerpUnclamped(start, target, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }

    public void SetLocked(bool value)
    {
        locked = value;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        if (locked)
        {
            rectTransform.anchoredPosition =
                originalPosition + Vector2.right * hoverOffset;
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void PlaySwipe()
    {
        if (SwipeSound == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        audioSource.PlayOneShot(SwipeSound);
    }
}
