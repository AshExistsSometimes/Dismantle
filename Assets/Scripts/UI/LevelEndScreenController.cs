using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEndScreenController : MonoBehaviour
{
    public static LevelEndScreenController Instance;

    [Header("Root")]
    public CanvasGroup root;

    [Header("Stats")]
    public StatUI time;
    public StatUI killCount;
    public StatUI score;

    [Header("Final Rank")]
    public TMP_Text finalRankText;
    public GameObject finalRankRoot;

    [Header("Prompt")]
    public GameObject prompt;

    [Header("Rank Colours")]
    public List<RankColor> rankColors = new();

    [Header("Timing")]
    public float fadeSpeed = 1.5f;
    public float stepDelay = 0.4f;
    public KeyCode continueKey = KeyCode.E;

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip tickLoop;     // looping ticking sound
    public AudioClip slamSound;    // for rank pop-in

    private bool waitingForInput = false;

    private void Awake()
    {
        Instance = this;
        SetUpEndScreen();
    }

    private void SetUpEndScreen()
    {
        root.alpha = 0f;
        root.gameObject.SetActive(false);

        killCount.root.SetActive(false);
        time.root.SetActive(false);
        score.root.SetActive(false);
        finalRankRoot.SetActive(false);
        prompt.SetActive(false);
    }

    public void Show()
    {
        root.gameObject.SetActive(true);
        root.alpha = 0f;

        var lm = LevelManager.Instance;

        // -------- RESET UI STATE --------

        time.root.SetActive(false);
        killCount.root.SetActive(false);
        score.root.SetActive(false);

        time.rankText.gameObject.SetActive(false);
        killCount.rankText.gameObject.SetActive(false);
        score.rankText.gameObject.SetActive(false);

        finalRankRoot.SetActive(false);
        prompt.SetActive(false);

        // Reset displayed values (so they animate from 0 properly)
        time.valueText.text = FormatTime(0f);
        killCount.valueText.text = "0";
        score.valueText.text = "0";

        // -------- SET FINAL TEXT (NO ANIMATION HERE) --------

        time.rankText.text = lm.TimeRank.ToString();
        killCount.rankText.text = lm.KillCountRank.ToString();
        score.rankText.text = lm.ScoreRank.ToString();

        finalRankText.text = lm.OverallRank.ToString();

        // -------- APPLY COLOURS --------

        ApplyRankColor(time.rankText, lm.TimeRank);
        ApplyRankColor(killCount.rankText, lm.KillCountRank);
        ApplyRankColor(score.rankText, lm.ScoreRank);
        ApplyRankColor(finalRankText, lm.OverallRank);

        // -------- LOCK PLAYER --------

        if (GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.enabled = false;
            GameManager.Instance.playerController.rb.linearVelocity = Vector3.zero;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        waitingForInput = false;

        // -------- START SEQUENCE --------
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        var lm = LevelManager.Instance;

        // -------- FADE IN --------
        while (root.alpha < 1f)
        {
            root.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // -------- TIME --------
        time.root.SetActive(true);

        yield return StartCoroutine(AnimateValue(time.valueText, 0.6f, (t) =>
        {
            float val = Mathf.Lerp(0f, lm.LevelEndTime, t);
            return FormatTime(val);
        }));

        yield return new WaitForSeconds(stepDelay);

        time.rankText.gameObject.SetActive(true);
        yield return StartCoroutine(SlamIn(time.rankText.transform));

        yield return new WaitForSeconds(stepDelay);


        // -------- KILL COUNT --------
        killCount.root.SetActive(true);

        yield return StartCoroutine(AnimateValue(killCount.valueText, 0.6f, (t) =>
        {
            int val = Mathf.RoundToInt(Mathf.Lerp(0, lm.LevelEndKilCount, t));
            return val.ToString();
        }));

        yield return new WaitForSeconds(stepDelay);

        killCount.rankText.gameObject.SetActive(true);
        yield return StartCoroutine(SlamIn(killCount.rankText.transform));

        yield return new WaitForSeconds(stepDelay);


        // -------- SCORE --------
        score.root.SetActive(true);

        yield return StartCoroutine(AnimateValue(score.valueText, 0.6f, (t) =>
        {
            int val = Mathf.RoundToInt(Mathf.Lerp(0, (int)lm.LevelEndScore, t));
            return val.ToString();
        }));

        yield return new WaitForSeconds(stepDelay);

        score.rankText.gameObject.SetActive(true);
        yield return StartCoroutine(SlamIn(score.rankText.transform));

        yield return new WaitForSeconds(stepDelay);


        // -------- FINAL RANK --------
        finalRankRoot.SetActive(true);
        yield return StartCoroutine(SlamIn(finalRankText.transform));

        yield return new WaitForSeconds(stepDelay);

        // -------- PROMPT --------
        prompt.SetActive(true);
        waitingForInput = true;
    }

    private void Update()
    {
        if (!waitingForInput) return;

        if (Input.GetKeyDown(continueKey))
        {
            waitingForInput = false;
            SetUpEndScreen();
            LevelManager.Instance.ReturnToHub();
        }
    }

    private void ApplyRankColor(TMP_Text txt, Rank rank)
    {
        foreach (var rc in rankColors)
        {
            if (rc.rank == rank)
            {
                txt.color = rc.color;
                return;
            }
        }
    }

    private string FormatTime(float t)
    {
        int hours = Mathf.FloorToInt(t / 3600f);
        int minutes = Mathf.FloorToInt((t % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int milliseconds = Mathf.FloorToInt((t * 100f) % 100f);

        return $"{hours:00}:{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    private IEnumerator AnimateValue(TMP_Text text, float duration, System.Func<float, string> formatter)
    {
        float time = 0f;

        // Start ticking loop
        if (tickLoop != null && audioSource != null)
        {
            audioSource.clip = tickLoop;
            audioSource.loop = true;
            audioSource.Play();
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            // Ease OUT (fast start, slow end)
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            text.text = formatter(eased);

            yield return null;
        }

        text.text = formatter(1f);

        // Stop ticking
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private IEnumerator SlamIn(Transform target)
    {
        float time = 0f;
        float duration = 0.2f;

        Vector3 startScale = Vector3.one * 1.8f;
        Vector3 endScale = Vector3.one;

        target.localScale = startScale;

        // Play slam SFX
        if (slamSound != null && audioSource != null)
            audioSource.PlayOneShot(slamSound);

        Camera.main.transform.position += Random.insideUnitSphere * 0.05f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Ease OUT with slight overshoot
            float eased = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);

            target.localScale = Vector3.LerpUnclamped(startScale, endScale, t) * eased;

            yield return null;
        }

        target.localScale = endScale;
    }
}

[System.Serializable]
public class RankColor
{
    public Rank rank;
    public Color color;
}

[System.Serializable]
public class StatUI
{
    public TMP_Text valueText;
    public TMP_Text rankText;
    public GameObject root;
}