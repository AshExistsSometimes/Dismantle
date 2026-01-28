using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueNodeSO> Nodes;
}

