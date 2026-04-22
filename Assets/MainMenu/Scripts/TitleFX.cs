using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
public class TitleFX : MonoBehaviour, IPointerClickHandler
{
    [Header("Pulse")]
    public bool usePulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;

    [Header("Rotation")]
    public bool useRotation = false;
    public float rotationSpeed = 10f;

    [Header("Click Feedback")]
    public float clickScaleMultiplier = 1.2f;
    public float clickDuration = 0.15f;

    [Header("Shake")]
    public float shakeDuration = 0.2f;
    public float shakeStrength = 5f;

    private Vector3 baseScale;
    private Quaternion baseRotation;
    private Vector3 basePosition;

    private void Awake()
    {
        baseScale = transform.localScale;
        baseRotation = transform.localRotation;
        basePosition = transform.localPosition;
    }

    private void Update()
    {
        // -------- PULSE --------
        if (usePulse)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = baseScale * pulse;
        }

        // -------- ROTATION --------
        if (useRotation)
        {
            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * rotationSpeed) * 2f);
        }
    }

    // -------- CLICK --------
    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ClickFeedback());
    }

    private IEnumerator ClickFeedback()
    {
        // punch scale up
        float t = 0f;
        Vector3 start = transform.localScale;
        Vector3 target = baseScale * clickScaleMultiplier;

        while (t < clickDuration)
        {
            t += Time.deltaTime;
            float lerp = t / clickDuration;
            transform.localScale = Vector3.Lerp(start, target, lerp);
            yield return null;
        }

        // return to normal
        t = 0f;
        while (t < clickDuration)
        {
            t += Time.deltaTime;
            float lerp = t / clickDuration;
            transform.localScale = Vector3.Lerp(target, baseScale, lerp);
            yield return null;
        }

        // shake
        yield return StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector2 offset = Random.insideUnitCircle * shakeStrength;
            transform.localPosition = basePosition + new Vector3(offset.x, offset.y, 0f);

            yield return null;
        }

        transform.localPosition = basePosition;
    }
}
