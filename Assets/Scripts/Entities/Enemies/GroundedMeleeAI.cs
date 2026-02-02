using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ground-based melee enemy with roaming, chasing, and playful interaction behaviour.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class GroundedMeleeAI : BaseEnemy
{
    [Header("References")]
    public Transform Head;

    [Header("Line of Sight")]
    public LayerMask SightBlockingLayers;
    public float LoseTargetDelay = 0.5f;

    private float loseTargetTimer;

    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private Transform playerTransform;

    private enum AIState
    {
        IdleRoam,
        Chase,
        Play
    }

    private AIState currentState = AIState.IdleRoam;

    [Header("Idle Roam")]
    public float IdleRoamDistance = 5f;
    public float IdlePauseTime = 2f;

    [Header("Head Look")]
    public float HeadTurnSpeed = 6f;
    public float MaxHeadYaw = 60f;

    [Header("Play Behaviour")]
    public float PlayDuration = 5f;
    public float PlayCooldown = 10f;
    public AnimationCurve PlaySpinCurve;
    public float MaxPlaySpinSpeed = 720f;

    [Header("Debug")]
    public bool DoDebugLogs = false;

    private Vector3 currentRoamTarget;
    private float idleTimer;
    private bool isIdlePaused;
    private float idleHeadTargetYaw;

    private Transform playTarget;
    private float playTimer;
    private float playCooldownTimer;

    private int playRotations;
    private float playSpinDegrees;
    private Quaternion headRestRotation;

    protected override void Awake()
    {
        base.Awake();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else if (DoDebugLogs)
        {
            Debug.LogWarning($"{name} could not find Player by tag.");
        }

        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;

        agent.speed = DefaultSpeed;
        agent.stoppingDistance = AttackDistance;
        agent.updateRotation = true;

        if (Head != null)
            headRestRotation = Head.localRotation;

        PickNewRoamPoint();
    }

    private void Update()
    {
        if (NoAI)
        {
            HaltAI();
            return;
        }

        playCooldownTimer -= Time.deltaTime;

        DetectTargetWithLOS();
        HandleAttack();

        UpdateState();
        UpdateMovement();
        UpdateHeadLook();
    }

    private void HaltAI()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void UpdateState()
    {
        // HARD LOCK: Play state owns itself
        if (currentState == AIState.Play)
            return;

        // Player always overrides everything
        if (target != null)
        {
            currentState = AIState.Chase;
            return;
        }

        // Only attempt Play from IdleRoam
        if (currentState == AIState.IdleRoam && TryEnterPlayState())
            return;

        currentState = AIState.IdleRoam;
    }


    private bool TryEnterPlayState()
    {
        if (currentState != AIState.IdleRoam)
            return false;

        if (playCooldownTimer > 0f)
            return false;

        Collider[] hits = Physics.OverlapSphere(transform.position, SightRange);
        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.name != name)
                continue;

            playTarget = hit.transform;
            playTimer = PlayDuration;
            playSpinDegrees = 0f;
            playRotations = Random.Range(1, 4);

            currentState = AIState.Play;

            if (DoDebugLogs)
                Debug.Log($"{name} entered Play with {playTarget.name}");

            return true;
        }

        return false;
    }

    private void UpdateMovement()
    {
        switch (currentState)
        {
            case AIState.Chase:
                agent.isStopped = false;
                agent.SetDestination(target.position);
                break;

            case AIState.IdleRoam:
                HandleIdleRoam();
                break;

            case AIState.Play:
                HandlePlayMovement();
                break;
        }
    }

    private void HandleIdleRoam()
    {
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
        agent.SetDestination(currentRoamTarget);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            isIdlePaused = true;
            idleTimer = IdlePauseTime;
            idleHeadTargetYaw = Random.Range(-MaxHeadYaw, MaxHeadYaw);
        }
    }

    private void HandlePlayMovement()
    {
        if (playTarget == null)
        {
            ExitPlayState();
            return;
        }

        playTimer -= Time.deltaTime;
        if (playTimer <= 0f)
        {
            ExitPlayState();
            return;
        }

        float dist = Vector3.Distance(transform.position, playTarget.position);

        if (dist > AttackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(playTarget.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void UpdateHeadLook()
    {
        if (Head == null)
            return;

        if (currentState == AIState.Play && playTarget != null)
        {
            HandlePlayHeadSpin();
            return;
        }

        float targetYaw = 0f;

        if (currentState == AIState.Chase && target != null)
        {
            targetYaw = GetYawToTarget(target.position);
        }
        else if (isIdlePaused)
        {
            targetYaw = idleHeadTargetYaw;
        }
        else if (agent.hasPath)
        {
            targetYaw = GetYawToTarget(agent.steeringTarget);
        }

        Head.localRotation = Quaternion.Slerp(
            Head.localRotation,
            Quaternion.Euler(0f, targetYaw, 0f),
            Time.deltaTime * HeadTurnSpeed
        );
    }

    private void HandlePlayHeadSpin()
    {
        if (playRotations <= 0)
            return;

        float totalDegrees = playRotations * 360f;
        float normalized = Mathf.Clamp01(playSpinDegrees / totalDegrees);

        float curveValue = PlaySpinCurve.Evaluate(normalized);

        float spinSpeed = curveValue * MaxPlaySpinSpeed;
        playSpinDegrees += spinSpeed * Time.deltaTime;

        float yaw = playSpinDegrees % 360f;

        Head.localRotation = headRestRotation * Quaternion.Euler(0f, yaw, 0f);
    }


    private float GetYawToTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - Head.position;
        dir.y = 0f;

        float yaw = Quaternion.LookRotation(dir).eulerAngles.y - transform.eulerAngles.y;
        return Mathf.Clamp(Mathf.DeltaAngle(0f, yaw), -MaxHeadYaw, MaxHeadYaw);
    }

    private void ExitPlayState()
    {
        playTarget = null;
        playCooldownTimer = PlayCooldown;
        currentState = AIState.IdleRoam;

        playSpinDegrees = 0f;

        if (Head != null)
            Head.localRotation = headRestRotation;

        PickNewRoamPoint();
    }

    private void PickNewRoamPoint()
    {
        Vector2 rnd = Random.insideUnitCircle * IdleRoamDistance;
        Vector3 pos = spawnPosition + new Vector3(rnd.x, 0f, rnd.y);

        if (NavMesh.SamplePosition(pos, out var hit, IdleRoamDistance, NavMesh.AllAreas))
            currentRoamTarget = hit.position;
        else
            currentRoamTarget = spawnPosition;
    }

    private void DetectTargetWithLOS()
    {
        if (playerTransform == null || Head == null)
            return;

        Vector3 origin = Head.position;
        Vector3 toPlayer = playerTransform.position - origin;
        float distance = toPlayer.magnitude;

        if (distance > SightRange)
        {
            HandleTargetLoss(Time.deltaTime);
            return;
        }

        Vector3 dir = toPlayer.normalized;

        // If something blocks LOS before the player, LOS is broken
        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, SightBlockingLayers))
        {
            HandleTargetLoss(Time.deltaTime);
            return;
        }

        target = playerTransform;
        loseTargetTimer = 0f;
    }

    private void HandleTargetLoss(float delta)
    {
        if (target == null)
            return;

        loseTargetTimer += delta;

        if (loseTargetTimer >= LoseTargetDelay)
        {
            if (DoDebugLogs)
                Debug.Log($"{name} lost target after LOS timeout");

            target = null;
            loseTargetTimer = 0f;

            // Prevent immediate Play re-entry
            playCooldownTimer = PlayCooldown;
        }
    }

}
