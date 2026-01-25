using System.Collections;
using UnityEngine;

public enum EnemyModifier { Common, Rare, Legendary }

public class BaseEnemy : Entity
{
    [Header("<color=#A61A21><size=110%><b>Enemy Stats")]
    public int Damage = 10;
    public EnemyModifier Modifier = EnemyModifier.Common;

    [Header("<color=#D71C74><size=110%><b>Detection")]
    public LayerMask targetLayer;
    public float SightRange = 10f;
    public float AttackDistance = 2f;

    [Header("<color=#FFC700><size=110%><b>Class Change")]
    public bool CanChangeClass = true;

    protected Transform target;
    protected float attackCooldown;

    private new void Awake()
    {
        base.Awake();

        // Attempt class change if allowed
        if (CanChangeClass)
            AttemptClassUpgrade();

        ApplyModifier();
    }

    protected virtual void Update()
    {
        if (NoAI) return;

        DetectTarget();
        HandleAttack();
    }

    // --------------------
    // Class Upgrade Logic
    // --------------------
    private void AttemptClassUpgrade()
    {
        float roll = Random.value; // 0 to 1

        switch (Modifier)
        {
            case EnemyModifier.Common:
                if (roll <= 0.01f)
                    Modifier = EnemyModifier.Legendary;
                else if (roll <= 0.06f) // 0.01 + 0.05
                    Modifier = EnemyModifier.Rare;
                break;

            case EnemyModifier.Rare:
                if (roll <= 0.02f)
                    Modifier = EnemyModifier.Legendary;
                break;

            case EnemyModifier.Legendary:
                // No change possible
                break;
        }
    }

    // --------------------
    // Modifier Logic
    // --------------------
    protected virtual void ApplyModifier()
    {
        switch (Modifier)
        {
            case EnemyModifier.Common:
                SetOutlineColor(Color.red);
                break;
            case EnemyModifier.Rare:
                MaxHP = Mathf.RoundToInt(MaxHP * 1.5f);
                HP = MaxHP;
                AttackDamage = Mathf.RoundToInt(AttackDamage * 1.5f);
                gameObject.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
                DefaultSpeed = DefaultSpeed * 0.9f;
                SetOutlineColor(new Color(0.6f, 0f, 1f)); // purple
                break;
            case EnemyModifier.Legendary:
                MaxHP = Mathf.RoundToInt(MaxHP * 2f);
                HP = MaxHP;
                AttackDamage = Mathf.RoundToInt(AttackDamage * 2f);
                gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                DefaultSpeed = DefaultSpeed * 0.8f;
                SetOutlineColor(Color.yellow);
                break;
        }
    }

    private void SetOutlineColor(Color color)
    {
        if (outline != null)
        {
            color.a = 1f; // fully visible
            outline.OutlineColor = color;
        }
    }

    // --------------------
    // Target Detection
    // --------------------
    protected virtual void DetectTarget()
    {
        if (target != null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, SightRange, targetLayer);
        if (hits.Length > 0)
            target = hits[0].transform;
    }

    // --------------------
    // Attack Logic
    // --------------------
    protected virtual void HandleAttack()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= AttackDistance && Time.time >= attackCooldown)
        {
            PerformAttack();
            attackCooldown = Time.time + (1f / AttackRate);
        }
    }

    protected virtual void PerformAttack()
    {
        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(Damage);
            Debug.Log($"{name} attacked {target.name} for {Damage} damage.");
        }
    }

    // --------------------
    // Gizmos for Debugging
    // --------------------
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SightRange);
    }
}
