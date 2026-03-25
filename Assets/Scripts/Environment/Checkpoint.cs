using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public LevelManager levelManager;
    public Transform myTransform;

    public bool DestroyOnTrigger = true;

    private void Awake()
    {
        levelManager = LevelManager.Instance;

        myTransform = gameObject.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) { return; }

        Debug.Log("Player Triggered Checkpoint at: " + myTransform);

        levelManager.SetNewCheckpoint(myTransform);
        levelManager.HasCheckpoint = true;

        if (DestroyOnTrigger)
        {
            gameObject.SetActive(false);
        }
    }
}
