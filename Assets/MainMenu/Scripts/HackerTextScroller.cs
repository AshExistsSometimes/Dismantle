using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HackerTextScroller : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textDisplay;
    public MainMenuManager mainMenuManager;

    [Header("Boot Sequence (plays once, in order)")]
    [TextArea]
    public List<string> bootLines = new List<string>();

    [Header("Looping Lines (random)")]
    [TextArea(3, 10)]
    public List<string> possibleLines = new List<string>();

    [Header("Line Settings")]
    public int maxLines = 10;

    [Header("Typewriter")]
    public float characterDelay = 0.02f;
    public float lineDelay = 0.4f;

    [Header("Visual")]
    public Color textColour = Color.green;
    public bool useFlicker = true;
    public float flickerAmount = 0.08f;

    [Header("Cursor")]
    public bool useCursor = true;
    public float cursorBlinkSpeed = 0.5f;
    public string cursorChar = "_";

    [Header("Glitch")]
    public float glitchChance = 0.03f;

    [Header("Error Injection")]
    public float errorChance = 0.08f;
    public string errorText = "!! ERROR: SIGNAL LOST !!";

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    public float soundPitchMin = 0.9f;
    public float soundPitchMax = 1.1f;

    private Queue<string> currentLines = new Queue<string>();
    private bool cursorVisible = true;

    private void Awake()
    {
        if (textDisplay == null)
            textDisplay = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        textDisplay.color = textColour;

        StartCoroutine(CursorBlink());
        StartCoroutine(MainRoutine());
    }

    private void Update()
    {
        if (useFlicker)
        {
            float flicker = 1f - flickerAmount + Random.value * flickerAmount;
            textDisplay.color = textColour * flicker;
        }
    }

    private IEnumerator MainRoutine()
    {
        // -------- BOOT SEQUENCE --------
        foreach (string block in bootLines)
        {
            string[] lines = block.Split('\n');

            foreach (string line in lines)
            {
                AddLineInstant(line);

                UpdateDisplay();

                yield return new WaitForSeconds(0.02f);
            }
        }

        // render once after full burst
        UpdateDisplay();

        // small pause before normal operation
        yield return new WaitForSeconds(0.5f);

        // -------- LOOP --------
        while (true)
        {
            if (possibleLines.Count == 0) yield break;

            string line = possibleLines[Random.Range(0, possibleLines.Count)];

            // occasional error injection
            if (Random.value < errorChance)
                line = errorText;

            yield return StartCoroutine(TypeLine(line));
            yield return new WaitForSeconds(lineDelay * 1.5f);
        }
    }

    private IEnumerator TypeLine(string block)
    {
        // Split into lines
        string[] lines = block.Split('\n');

        for (int l = 0; l < lines.Length; l++)
        {
            string line = lines[l];

            // Add new line slot
            currentLines.Enqueue("");

            while (currentLines.Count > maxLines)
                currentLines.Dequeue();

            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                // glitch during typing
                if (Random.value < glitchChance)
                    c = (char)Random.Range(33, 126);

                current += c;

                SetLastLine(current);
                UpdateDisplay();

                PlayTypeSound();

                yield return new WaitForSeconds(characterDelay);
            }

            // finalize clean line (removes glitches)
            SetLastLine(line);
            UpdateDisplay();

            yield return new WaitForSeconds(lineDelay * 0.5f); // slight pause between lines in same block
        }
    }

    private void AddLineInstant(string line)
    {
        currentLines.Enqueue(line);

        while (currentLines.Count > maxLines)
            currentLines.Dequeue();
    }

    private void SetLastLine(string value)
    {
        string[] lines = currentLines.ToArray();
        lines[lines.Length - 1] = value;

        currentLines.Clear();
        for (int i = 0; i < lines.Length; i++)
            currentLines.Enqueue(lines[i]);
    }

    private void UpdateDisplay()
    {
        string text = string.Join("\n", currentLines);

        // Cursor should NOT create a new line
        if (useCursor && cursorVisible)
            text += cursorChar;

        textDisplay.text = text;
    }

    private void ReplaceLastLine(string finalLine)
    {
        string[] lines = currentLines.ToArray();
        lines[lines.Length - 1] = finalLine;
        currentLines = new Queue<string>(lines);
    }

    private IEnumerator CursorBlink()
    {
        while (true)
        {
            cursorVisible = !cursorVisible;
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }

    private void PlayTypeSound()
    {
        if (audioSource == null || typeSound == null) return;

        audioSource.pitch = Random.Range(soundPitchMin, soundPitchMax);
        audioSource.PlayOneShot(typeSound);
    }

    public void UpdateColour()
    {
        textColour = mainMenuManager.MenuAccentColour;
        textDisplay.color = textColour;
    }
}