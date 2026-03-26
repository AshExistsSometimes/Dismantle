using UnityEngine;

public class FlyingAI : BaseEnemy
{
    [Header("References")]
    public Transform Body;
    public Transform GunPivot;
    public Transform FirePoint;

    [Header("Projectile")]
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 25f;

    [Header("Movement")]
    public Vector2 AttemptedRangeFromPlayer = new Vector2(6f, 10f);
    public float HeightAbovePlayer = 2f;
    public float RoamRadius = 8f;
    public float MoveSpeed = 5f;

    [Header("LOS")]
    public LayerMask SightBlockingLayers;

    private Transform player;
    private Vector3 spawnPos;
    private Vector3 targetPos;

    protected override void Awake()
    {
        base.Awake();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnPos = transform.position;

        PickRoamPoint();
    }

    private void Update()
    {
        if (NoAI || player == null) return;

        bool hasLOS = HasLOS();

        if (!hasLOS)
        {
            HandleRoam();
        }
        else
        {
            HandleCombatMovement();
            HandleAttack();
        }

        Move();
        Aim();
    }

    // --------------------
    // LOS
    // --------------------

    private bool HasLOS()
    {
        Vector3 dir = player.position - FirePoint.position;
        float dist = dir.magnitude;

        if (Physics.Raycast(FirePoint.position, dir.normalized, dist, SightBlockingLayers))
            return false;

        return true;
    }

    // --------------------
    // Movement
    // --------------------

    private void HandleRoam()
    {
        if (Vector3.Distance(transform.position, targetPos) < 1f)
            PickRoamPoint();
    }

    private void HandleCombatMovement()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        Vector3 desiredPos = transform.position;

        // Maintain distance
        if (dist < AttemptedRangeFromPlayer.x)
        {
            Vector3 away = (transform.position - player.position).normalized;
            desiredPos = transform.position + away * 2f;
        }
        else if (dist > AttemptedRangeFromPlayer.y)
        {
            desiredPos = player.position;
        }
        else
        {
            desiredPos = transform.position;
        }

        // always try to stay above player
        desiredPos.y = player.position.y + HeightAbovePlayer;

        targetPos = desiredPos;
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            MoveSpeed * Time.deltaTime
        );
    }

    // --------------------
    // Aiming
    // --------------------

    private void Aim()
    {
        if (player == null) return;

        // BODY (Y rotation only)
        Vector3 flat = player.position - Body.position;
        flat.y = 0f;

        if (flat.sqrMagnitude > 0.01f)
        {
            Quaternion bodyRot = Quaternion.LookRotation(flat);
            Body.rotation = Quaternion.Slerp(Body.rotation, bodyRot, Time.deltaTime * 6f);
        }

        // GUN (sideways pivot)
        Vector3 dir = player.position - GunPivot.position;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Convert to local space relative to body
        Quaternion localRot = Quaternion.Inverse(Body.rotation) * lookRot;
        Vector3 euler = localRot.eulerAngles;

        float pitch = Mathf.DeltaAngle(0f, euler.x);

        // Clamp vertical aim
        pitch = Mathf.Clamp(pitch, -28f, 90f);

        // Apply (forward = Z 90)
        GunPivot.localRotation = Quaternion.Euler(pitch, 0f, 90f);
    }

    // --------------------
    // Attack
    // --------------------

    protected override void HandleAttack()
    {
        if (player == null) return;
        if (!HasLOS()) return;
        if (Time.time < attackCooldown) return;

        PerformAttack();
        attackCooldown = Time.time + (1f / AttackRate);
    }

    protected override void PerformAttack()
    {
        if (ProjectilePrefab == null || FirePoint == null) return;

        GameObject proj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.Initialize(
                AttackDamage,
                FirePoint.forward,
                ProjectileSpeed,
                true
            );
        }
    }

    // --------------------
    // Roaming
    // --------------------

    private void PickRoamPoint()
    {
        Vector2 rnd = Random.insideUnitCircle * RoamRadius;

        targetPos = spawnPos + new Vector3(
            rnd.x,
            Random.Range(1f, 3f),
            rnd.y
        );
    }
}