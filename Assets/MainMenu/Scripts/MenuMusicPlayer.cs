using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicPlayer : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    private void Start()
    {
        if (musicClip != null)
            audioSource.Play();
    }
}