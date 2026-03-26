using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public LevelManager levelManager;
    public Transform myTransform;

    public bool DestroyOnTrigger = true;

    public ParticleSystem confetti;

    public float VisualRespawnTimer = 10f;
    public float DestroyTimer = 5f;

    private bool Visible = true;

    private void Awake()
    {
        levelManager = LevelManager.Instance;

        myTransform = gameObject.transform;

        confetti.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) { return; }

        confetti.Play();

        Debug.Log("Player Triggered Checkpoint at: " + myTransform);

        levelManager.SetNewCheckpoint(myTransform);
        levelManager.HasCheckpoint = true;

        if (DestroyOnTrigger)
        {
            StartCoroutine(WaitToDestroy());
        }
    }

    private IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(DestroyTimer);
        gameObject.SetActive(false);
    }

}
