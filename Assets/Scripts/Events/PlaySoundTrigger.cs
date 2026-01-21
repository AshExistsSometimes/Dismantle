using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlaySoundTrigger : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip sound;
    public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Ensure trigger collider
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (sound == null)
            return;

        audioSource.PlayOneShot(sound, volume);
    }
}
