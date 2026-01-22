using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamagable
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

    public float PassiveRegenRate;
    public float PassiveRegenDelay;
    public int PassiveRegenAmount;

    [Header("Defaults")]
    public int DefaultMaxHP = 100;

    public float DefaultPassiveRegenRate = 50f;
    public float DefaultPassiveRegenDelay = 0;
    public int DefaultPassiveRegenAmount = 5;

    public int DefaultArmour = 0;

    [Header("Settings")]
    public bool PassiveRegeneration = false;

    private Coroutine regenCoroutine = null;
    [SerializeField]
    private float RegenDelayTimer;

    //

    private void Start()
    {
        ResetValuesToDefault();// Bad for final game, values will need to be set to be accurate to the modifiers the player has active, defaults are meant to be the values the stats get set to when modifiers are removed
    }

    private void Update()
    {
        if (!PassiveRegeneration) { return; }

        UpdatePassiveRegenTimer();
        if (RegenDelayTimer <= 0f && regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenToFull(PassiveRegenRate / 100f));
        }
    }

    //

    public void ResetValuesToDefault()
    {
        MaxHP = DefaultMaxHP;
        CurrentHP = MaxHP;

        Armour = DefaultArmour;

        PassiveRegenAmount = DefaultPassiveRegenAmount;
        PassiveRegenDelay = DefaultPassiveRegenDelay;
        PassiveRegenRate = DefaultPassiveRegenRate;

        if (PassiveRegeneration)
        {
            RegenDelayTimer = PassiveRegenDelay;
        }
    }

    public void ConvertArmourToDecimal()// Run when armour value changes
    {
        damageReduction = ArmourDamageReductionPercent / 100f;
    }

    public void TakeDamage(int amount)
    {
        if (PassiveRegeneration)
        {
            RegenDelayTimer = PassiveRegenDelay;
        }

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        CheckArmourRemaining();

        if (hasArmourRemaining)
        {
            // If there is armour remaining, take damage equal to amount * damageReduction, rounded to nearest int
            int reducedDamage = Mathf.RoundToInt(amount * (1f - damageReduction));
            
            CurrentHP -= reducedDamage;
        }
        else
        {
            // standard damage application
            CurrentHP -= amount;
        }

        if (CurrentHP <= 0) { Die(); return; }          
    }

    public void CheckArmourRemaining()
    {
        if (CurrentHP > (MaxHP - Armour))
        {
            hasArmourRemaining = true;
        }
        else
        {
            hasArmourRemaining = false;
        }
    }


    public void Die()
    {
        // fade in death screen and turn off player controller and duel hooks scripts (LEAVE THIS FOR NOW)
        Debug.Log("[!] : Player has died");
    }


    // REGENERATION

    public IEnumerator RegenHPByAmount(int HealAmount, float regenRate)
    {
        int finalHP = CurrentHP + HealAmount;
        if (finalHP > MaxHP) { finalHP = MaxHP; }

        while (CurrentHP < finalHP)
        {
            CurrentHP += 1;
            yield return new WaitForSeconds(regenRate);
        }

        regenCoroutine = null;
    }

    public IEnumerator RegenToFull(float regenRate)
    {
        while (CurrentHP < MaxHP)
        {
            CurrentHP += 1;
            yield return new WaitForSeconds(regenRate);
        }

        regenCoroutine = null;
    }

    public void UpdatePassiveRegenTimer()
    {
        if (RegenDelayTimer > 0f)
        {
            RegenDelayTimer -= Time.deltaTime;
        }        
        else
        { RegenDelayTimer = 0f; }
    }
}
