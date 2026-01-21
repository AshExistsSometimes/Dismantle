using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScrollText : MonoBehaviour
{
    [Header("Text Settings")]
    [TextArea]
    public string StringText;
    [Space]
    public int MaxCharactersVisible = 10;
    [Space]
    [Header("Scroll Settings")]
    public float ScrollSpeed = 5f;
    public float DelayBetweenScrolls = 1f;
    [Space]
    public bool ScrollRight = false;
    public bool LoopSeamlessly = true;

    private TextMeshProUGUI textUI;
    private Coroutine scrollCoroutine;
    private float scrollPos;


    private void Awake()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        StartScrolling();
    }

    private void OnDisable()
    {
        StopScrolling();
    }

    public void StartScrolling()
    {
        StopScrolling();
        scrollPos = 0f;
        scrollCoroutine = StartCoroutine(ScrollRoutine());
    }

    public void StopScrolling()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    private IEnumerator ScrollRoutine()
    {
        if (string.IsNullOrEmpty(StringText) || MaxCharactersVisible <= 0)
        {
            textUI.text = string.Empty;
            yield break;
        }

        int textLength = StringText.Length;
        float stepTime = 1f / Mathf.Max(ScrollSpeed, 0.01f);

        while (true)
        {
            if (LoopSeamlessly)
            {
                AdvanceSeamless(textLength);
                yield return new WaitForSeconds(stepTime);
            }
            else
            {
                yield return StartCoroutine(NonSeamlessScroll(textLength, stepTime));
                textUI.text = string.Empty;
                yield return new WaitForSeconds(DelayBetweenScrolls);
                scrollPos = 0f;
            }
        }
    }

    private void AdvanceSeamless(int textLength)
    {
        scrollPos += ScrollRight ? -1f : 1f;

        if (scrollPos < 0f)
            scrollPos += textLength;

        scrollPos %= textLength;

        textUI.text = BuildWindow((int)scrollPos, textLength);
    }

    private IEnumerator NonSeamlessScroll(int textLength, float stepTime)
    {
        int maxSteps = textLength + MaxCharactersVisible;

        for (int i = 0; i < maxSteps; i++)
        {
            int startIndex = ScrollRight
                ? textLength - i
                : i;

            textUI.text = BuildWindow(startIndex, textLength);
            yield return new WaitForSeconds(stepTime);
        }
    }

    private string BuildWindow(int startIndex, int textLength)
    {
        StringBuilder builder = new StringBuilder(MaxCharactersVisible);

        for (int i = 0; i < MaxCharactersVisible; i++)
        {
            int index = (startIndex + i) % textLength;
            if (index < 0)
                index += textLength;

            builder.Append(StringText[index]);
        }

        return builder.ToString();
    }
}
