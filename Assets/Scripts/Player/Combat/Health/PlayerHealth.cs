using System.ComponentModel;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int CurrentHP = 100;

    public int MaxHP;

    public int Armour;
    [Range (0f, 100f)]
    public float ArmourDamageReductionPercent = 50f;
    [HideInInspector]
    public float damageReduction;
    private bool hasArmourRemaining;

    public float RegenRate;
    public float RegenDelay;
    public int RegenAmount;

    [Header("Defaults")]
    public int DefaultMaxHP = 100;

    public float DefaultRegenRate = 50f;
    public float DefaultRegenDelay = 0; // if 0, passive regen is off
    public int DefaultRegenAmount = 5;

    public int DefaultArmour = 0;

    //

    public void ConvertArmourToDecimal()
    {
        damageReduction = ArmourDamageReductionPercent / 100f;
    }

    public void TakeDamage(float amount)
    {
        if (hasArmourRemaining)
        {
            // Need to convert betwen ints and floats
        }
    }
}
