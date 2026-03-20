using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ground-based ranged enemy using NavMeshAgent.
/// Rotates body on Y axis, arm on X axis, fires projectiles at player.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class GroundedRangedAI : BaseEnemy
{
    [Header("Hierarchy References")]
    public Transform Body;
    public Transform Arm;
    public Transform FirePoint;

    [Header("Projectile")]
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 25f;

    [Header("Ranged Behaviour")]
    public float PreferredMinDistance = 6f;
    public float PreferredMaxDistance = 10f;
    public float MaxMemoryDistance = 75f;

    [Header("Line of Sight")]
    public LayerMask SightBlockingLayers;
    public float LoseTargetDelay = 0.75f;

    [Header("Idle Roam")]
    public float IdleRoamRadius = 6f;
    public float IdlePauseTime = 2f;
    public float IdleStoppingDistance = 0.2f;

    private NavMeshAgent agent;
    private Vector3 spawnPosition;

    private float loseTargetTimer;
    private float idleTimer;
    private bool isIdlePaused;
    private Vector3 roamTarget;

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;

        agent.speed = DefaultSpeed;
        agent.stoppingDistance = PreferredMinDistance;

        PickNewRoamPoint();
    }

    private void Update()
    {
        if (NoAI)
        {
            HaltAI();
            return;
        }

        DetectTargetWithLOS();
        HandleAttack();

        UpdateMovement();
        UpdateAiming();
    }

    /// <summary>
    /// Stops NavMesh movement.
    /// </summary>
    private void HaltAI()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    // --------------------
    // Detection
    // --------------------

    private void DetectTargetWithLOS()
    {
        if (target != null)
        {
            HandleTargetPersistence();
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, SightRange, targetLayer);
        if (hits.Length == 0)
            return;

        Transform candidate = hits[0].transform;

        if (!HasLineOfSight(candidate))
            return;

        target = candidate;
        loseTargetTimer = 0f;
    }

    private void HandleTargetPersistence()
    {
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > MaxMemoryDistance || !HasLineOfSight(target))
        {
            loseTargetTimer += Time.deltaTime;
            if (loseTargetTimer >= LoseTargetDelay)
            {
                target = null;
                loseTargetTimer = 0f;
            }
        }
        else
        {
            loseTargetTimer = 0f;
        }
    }

    private bool HasLineOfSight(Transform t)
    {
        Vector3 origin = FirePoint.position;
        Vector3 dir = (t.position - origin).normalized;
        float dist = Vector3.Distance(origin, t.position);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, SightBlockingLayers))
            return false;

        return true;
    }

    // --------------------
    // Movement
    // --------------------

    private void UpdateMovement()
    {
        if (target == null)
        {
            HandleIdleRoam();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        agent.isStopped = false;

        if (distance < PreferredMinDistance)
        {
            Vector3 retreatDir = (transform.position - target.position).normalized;
            Vector3 retreatPos = transform.position + retreatDir * 2f;
            agent.SetDestination(retreatPos);
        }
        else if (distance > PreferredMaxDistance)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void HandleIdleRoam()
    {
        agent.stoppingDistance = IdleStoppingDistance;

        if (isIdlePaused)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdlePaused = false;
                PickNewRoamPoint();
            }
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(roamTarget);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            isIdlePaused = true;
            idleTimer = IdlePauseTime;
        }
    }


    private void PickNewRoamPoint()
    {
        Vector2 rnd = Random.insideUnitCircle * IdleRoamRadius;
        Vector3 pos = spawnPosition + new Vector3(rnd.x, 0f, rnd.y);

        if (NavMesh.SamplePosition(pos, out var hit, IdleRoamRadius, NavMesh.AllAreas))
            roamTarget = hit.position;
        else
            roamTarget = spawnPosition;
    }

    // --------------------
    // Aiming
    // --------------------

    private void UpdateAiming()
    {
        if (target == null)
            return;

        // Body rotation (Y axis)
        Vector3 flatDir = target.position - Body.position;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude > 0.01f)
        {
            Quaternion bodyRot = Quaternion.LookRotation(flatDir);
            Body.rotation = Quaternion.Slerp(Body.rotation, bodyRot, Time.deltaTime * 8f);
        }

        // Arm rotation (X axis)
        Vector3 armDir = target.position - Arm.position;
        Quaternion armRot = Quaternion.LookRotation(armDir);
        Vector3 localEuler = armRot.eulerAngles;

        Arm.localRotation = Quaternion.Euler(-localEuler.x, 0f, 0f);
    }

    // --------------------
    // Attack
    // --------------------

    protected override void HandleAttack()
    {
        if (target == null)
            return;

        if (!HasLineOfSight(target))
            return;

        if (Time.time < attackCooldown)
            return;

        PerformAttack();
        attackCooldown = Time.time + (1f / AttackRate);
    }
    protected override void PerformAttack()
    {
        Debug.Log("Attempting to Attack");
        if (ProjectilePrefab == null || FirePoint == null)
        {
            Debug.Log("Attack Failed, Missing Reference");
            return;
        }
        Debug.Log("Attack Successful");

        GameObject projObj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);

        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(
                AttackDamage,
                FirePoint.forward,
                ProjectileSpeed,
                true
            );
        }
    }
}
