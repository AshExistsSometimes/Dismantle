using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Bounty")]
public class BountySO : ScriptableObject
{
    public string BountyName;
    public List<LevelSO> Levels = new();
}
