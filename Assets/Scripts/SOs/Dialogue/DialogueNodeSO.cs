using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Node")]
public class DialogueNodeSO : ScriptableObject
{
    public CharacterSO Speaker;
    public string Emotion;

    [TextArea(3, 6)]
    public string DialogueText;

    public bool HasButtons;
    public List<DialogueButton> Buttons;

    [Header("Typing")]
    public float CharactersPerSecondOverride = -1f;
}

[System.Serializable]
public class DialogueButton
{
    public string ButtonText;
    public UnityEvent ButtonEvent;
}
