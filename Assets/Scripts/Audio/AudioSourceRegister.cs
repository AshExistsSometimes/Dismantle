using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceRegister : MonoBehaviour
{
    public AudioType type;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Register(source, type);
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Unregister(source);
    }
}
