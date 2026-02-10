using UnityEngine;

[CreateAssetMenu(fileName = "WeaponColourSO", menuName = "Weapon Customisation/Weapon Colour")]
public class WeaponColourSO : ScriptableObject
{
    public string ColourName = "Colour";
    public Color Colour = Color.white;
    [Range(0f, 1f)]
    public float Smoothness = 1.0f;
    [Space]
    public bool OverrideMaterial = false; // if true, the segment of the gun that is being changed needs its material cached
                                          // so it can be restored, then the material is set to the new material.
                                          // This allows for colour variants like void, galaxy etc.
    public Material specialMaterial = null;
}
