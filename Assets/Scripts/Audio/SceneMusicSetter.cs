using UnityEngine;

public class SceneMusicSetter : MonoBehaviour
{
    public AudioClip baseMusic;

    public AudioClip combatMusic;

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetLevelMusic(baseMusic, combatMusic);
        }
    }
}