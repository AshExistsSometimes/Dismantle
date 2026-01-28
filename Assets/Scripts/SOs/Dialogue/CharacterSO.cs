using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Identity")]
    public string CharacterName;

    public List<EmotionIcon> Emotions;


    [Header("Voice")]
    public AudioClip VoiceBlip;
    public Vector2 PitchRange = new Vector2(0.95f, 1.05f);
}

[System.Serializable]
public struct EmotionIcon
{
    public string EmotionName;
    public Sprite Icon;
}

