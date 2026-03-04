using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneTrigger : MonoBehaviour
{
    private bool triggered;

    public string SceneToLoad;
    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (!other.CompareTag("Player")) return;

        triggered = true;

        SceneManager.LoadScene(SceneToLoad);
    }
}
