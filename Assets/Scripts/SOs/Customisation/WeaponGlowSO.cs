using UnityEngine;

[CreateAssetMenu(fileName = "WeaponColourSO", menuName = "Weapon Customisation/Weapon Glow")]
public class WeaponGlowSO : ScriptableObject
{
    public string GlowName = "Glow";
    public Color GlowColour = Color.cyan;// NEEDS TO BE IN HSV
}
