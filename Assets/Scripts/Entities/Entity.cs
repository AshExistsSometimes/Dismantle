using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class Entity : MonoBehaviour, IDamagable
{
    // VARIABLES //

    // IDENTITY
    [Header("<color=#42f2ff><size=110%><b>Identity")]

    public string Name = "Entity";

    public EntityType type;
    public enum EntityType
    {
        Hostile,
        Boss,
        NPC
    }

    // OUTLINE
    [Header("<color=#9b34eb><size=110%><b>Outline")]
    public bool OutlineEnabled = true;
    [Space]
    public Outline outline;
    public float MaxOutlineThickness = 3f;
    public OutlineColourSet OutlineColours;

    private float outlineThickness = 3f;
    private Transform player;

    [System.Serializable]
    public struct OutlineColourSet
    {
        public Color Hostile;
        public Color Boss;
        public Color NPC;

        public Color Get(EntityType type)
        {
            return type switch
            {
                EntityType.Boss => Boss,
                EntityType.NPC => NPC,
                _ => Hostile
            };
        }
    }

    // COMBAT
    [Header("<color=#ff6842><size=110%><b>Combat")]

    public int HP = 100;
    public int MaxHP = 100;
    [Space]
    public int AttackDamage = 5;
    public float AttackRate = 1f;
    [Space]
    public bool regenHP = false;
    public float HPRegenRate = 1f;
    public int HPRegenAmount = 1;

    private Coroutine regenCoroutine = null;

    // MOVEMENT
    [Header("<color=#ffec70><size=110%><b>Movement")]

    public float DefaultSpeed = 5f;

    // BUFFS / DEBUFFS
    [Header("<color=#f280ff><size=110%><b>Buffs / Debuffs")]

    public Effects effects;
    public enum Effects// NEEDS CHANGING - AN ENTITY SHOULD BE ABLE TO HAVE MULTIPLE BUFFS AT THE SAME TIME
    {
        Antiheal,// Prevents healing while active
        Overheat,// Deals fire damage over time - ENEMY ONLY
        MemoryLeak,// Slows target for a time - ENEMY ONLY
        Malware, // Stops target and deals damage for a time - ENEMY ONLY
    }


    // DEBUG
    [Header("<color=#ffb854><size=110%><b>Debug")]

    [Tooltip("Turns off the entities AI, preventing pathfinding and combat")]
    public bool NoAI = false;

    [Tooltip("Prevents entity from dying")]
    public bool Essential = false;

    [Tooltip("Prevents entity from taking damage at all")]
    public bool Immortal = false;

    // UNITY FUNCTIONS //

    private void Awake()
    {
        EnsureOutline();
        CachePlayer();
        outlineThickness = MaxOutlineThickness;
    }

    private void Update()
    {
        UpdateOutline();
    }

    // FUNCTIONS

    public virtual void TakeDamage(int damage)
    {
        if (Immortal) { return; }

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        HP -= damage;

        if (HP <= 0 && !Essential) { Die(); }
        else if (HP <= 0 && Essential && regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenToFull(HPRegenRate / 100f));
        }
    }

    public virtual void Die()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator RegenHPByAmount(int HealAmount, float regenRate)
    {
        int finalHP = HP + HealAmount;
        if (finalHP > MaxHP) { finalHP = MaxHP; }

        while (HP < finalHP)
        {
            HP += 1;
            yield return new WaitForSeconds(regenRate);
        }

        regenCoroutine = null;
    }

    public IEnumerator RegenToFull(float regenRate)
    {
        while (HP < MaxHP)
        {
            HP += 1;
            yield return new WaitForSeconds(regenRate);
        }

        regenCoroutine = null;
    }


    public void DisplayEffects()
    {
        // Display effect visuals (does nothing until I add effects)
    }

    private void UpdateOutline()
    {
        if (!OutlineEnabled || outline == null || player == null)
        {
            if (outline != null)
                outline.OutlineWidth = 0f;

            return;
        }

        UpdateOutlineColour();
        UpdateOutlineThickness();
    }

    private void UpdateOutlineColour()
    {
        outline.OutlineColor = OutlineColours.Get(type);
    }

    private void UpdateOutlineThickness()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // <= 10m = full thickness
        if (distance <= 10f)
        {
            outlineThickness = MaxOutlineThickness;
        }
        else
        {
            // 10m -> 30m fade to zero
            float t = Mathf.InverseLerp(10f, 30f, distance);
            outlineThickness = Mathf.Lerp(MaxOutlineThickness, 0f, t);
        }

        outline.OutlineWidth = outlineThickness;
    }

    private void EnsureOutline()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
            if (outline == null)
                outline = gameObject.AddComponent<Outline>();
        }

        outline.OutlineMode = Outline.Mode.OutlineVisible;
    }

    private void CachePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
}