using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenController : MonoBehaviour
{
    public static DeathScreenController Instance;

    [Header("UI")]
    public CanvasGroup root;
    public Image background;
    public GameObject deathText;
    public GameObject prompt;

    [Header("Settings")]
    public float fadeSpeed = 2f;
    public KeyCode continueKey = KeyCode.E;

    private bool waitingForInput = false;

    private void Awake()
    {
        Instance = this;

        root.alpha = 0f;
        root.gameObject.SetActive(false);

        deathText.SetActive(false);
        prompt.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        root.gameObject.SetActive(true);

        GameManager.Instance.PlayerDead = true;

        // HARD LOCK PLAYER
        if (GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.enabled = false;
            GameManager.Instance.playerController.rb.linearVelocity = Vector3.zero;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        deathText.SetActive(true);

        while (root.alpha < 1f)
        {
            root.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        prompt.SetActive(true);
        waitingForInput = true;
    }

    private void Update()
    {
        if (!waitingForInput) return;

        if (Input.GetKeyDown(continueKey))
        {
            waitingForInput = false;
            Respawn();
        }
    }

    private void Respawn()
    {
        root.alpha = 0f;
        root.gameObject.SetActive(false);

        deathText.SetActive(false);
        prompt.SetActive(false);

        waitingForInput = false;

        LevelManager.Instance.LoadLastCheckpoint();
    }
}