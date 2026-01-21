using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualHooks : MonoBehaviour
{
    [Header("References")]
    public List<LineRenderer> lineRenderers;
    public List<Transform> gunTips;
    public Transform cam;
    public Transform player;
    public LayerMask whatIsGrappleable;
    public PlayerController pm;

    [Header("Fuel")]
    public float maxFuel = 6f;
    public float fuel;
    public float singleHookCost = 1f;
    public float dualHookCost = 2f;
    public float fuelRechargeDelay = 0.6f;
    public float fuelRechargeRate = 1f;

    [Header("Swinging")]
    public float maxSwingDistance = 25f;

    [Header("Grapple Settings")]
    public float GrappleSpeed = 25f;
    public float GrappleReleaseDistance = 2f;
    public float DistanceSpeedModifier = 0.5f;
    public float MinGrappleSpeed = 18f;
    public float MaxGrappleSpeed = 60f;
    public float GrappleWindow = 0.2f;
    private float grappleTimer = 0f;
    public float MaxGrappleTime = 1.5f;
    [Space]
    public AnimationCurve grappleAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float ReelInSpeed = 6f;
    [Range(0f, 180f)]
    public float AdditiveGrappleAngle = 15f;

    private Vector3 grappleInitialVelocity; // stores velocity at grapple start
    private Vector3 grappleTargetVelocity;  // target velocity along grapple direction

    private float FinalGrappleVelocity;
    private Vector3 grappleStartPosition;
    private bool hasPassedStartDistance;
    private float gravityRestoreDelay = 0.1f;

    // Grapple motion tracking
    private Vector3 lastVelocitySample;
    private float velocitySampleTimer;
    private bool hasVelocityChangedEnough;

    private bool grappleAdditive = false;
    private float grappleProgress = 0f;

    [Header("ODM Gear")]
    public Rigidbody rb;

    [Header("Spring Settings")]
    public Vector2 swingDistanceRatio = new Vector2(0.25f, 0.8f);
    public float springForce = 15f;
    public float springDamping = 7f;
    public float springMassScale = 4.5f;

    [Header("Prediction")]
    public float sideBiasStrictness = 2f;
    public int minSamplesPerSide = 4;
    public int maxSamplesPerSide = 8;
    public float velocitySampleThreshold = 15f;
    public float GrappleDistanceLeniency = 10f;
    public List<Transform> predictionPoints;

    [Header("Input")]
    public KeyCode leftHookKey = KeyCode.Mouse0;
    public KeyCode rightHookKey = KeyCode.Mouse1;
    public KeyCode predictionModifierKey = KeyCode.LeftShift;

    [Header("Dual Swinging")]
    public int hookCount = 2;

    private Camera cachedCamera;

    private List<RaycastHit> predictedHits;
    private List<Vector3> hookWorldPoints;
    private List<SpringJoint> hookJoints;
    private List<bool> hookActive;
    private List<Vector3> ropeVisualPositions;

    private RaycastHit[] raycastBuffer = new RaycastHit[1];
    private int predictionFrameParity;

    private float fuelRechargeTimer;

    private bool dualGrappleActive = false;
    private Vector3 grappleMidpoint;
    private float grappleSpeed;

    private float lastLeftHookTime = -10f;
    private float lastRightHookTime = -10f;

    private bool grappleVisualActive = false;

    private void Awake()
    {
        cachedCamera = cam.GetComponent<Camera>();
    }

    private void Start()
    {
        predictedHits = new List<RaycastHit>(hookCount);
        hookWorldPoints = new List<Vector3>(hookCount);
        hookJoints = new List<SpringJoint>(hookCount);
        hookActive = new List<bool>(hookCount);
        ropeVisualPositions = new List<Vector3>(hookCount);

        for (int i = 0; i < hookCount; i++)
        {
            predictedHits.Add(default);
            hookWorldPoints.Add(Vector3.zero);
            hookJoints.Add(null);
            hookActive.Add(false);
            ropeVisualPositions.Add(Vector3.zero);
        }

        fuel = maxFuel;
    }

    private void Update()
    {
        if (!dualGrappleActive)
            HandleHookInput();

        if (dualGrappleActive)
            ApplyDualGrappleMovement();

        // Fuel recharge
        if (!dualGrappleActive && !hookActive[0] && !hookActive[1])
        {
            if (fuelRechargeTimer > 0f)
                fuelRechargeTimer -= Time.deltaTime;
            else
                fuel = Mathf.Min(fuel + fuelRechargeRate * Time.deltaTime, maxFuel);
        }

        bool predictionHeld = Input.GetKey(predictionModifierKey);
        predictionFrameParity ^= 1;

        if (predictionHeld && predictionFrameParity == 0)
            UpdatePredictionPoints();
        else if (!predictionHeld)
            DisablePredictionPoints();
    }

    private void LateUpdate()
    {
        UpdateRopeVisuals();
    }

    private void HandleHookInput()
    {
        if (Input.GetKeyDown(leftHookKey))
        {
            lastLeftHookTime = Time.time;
            TryStartHook(0);
        }

        if (Input.GetKeyDown(rightHookKey))
        {
            lastRightHookTime = Time.time;
            TryStartHook(1);
        }

        if (hookActive[0] && hookActive[1] && !dualGrappleActive)
        {
            if (Mathf.Abs(lastLeftHookTime - lastRightHookTime) <= GrappleWindow)
                StartDualGrapple();
        }

        if (Input.GetKeyUp(leftHookKey))
            StopHook(0);

        if (Input.GetKeyUp(rightHookKey))
            StopHook(1);
    }

    private void StartDualGrapple()
    {
        dualGrappleActive = true;
        grappleVisualActive = true;

        for (int i = 0; i < hookCount; i++)
            lineRenderers[i].positionCount = 2;

        grappleTimer = 0f;
        velocitySampleTimer = 0f;
        hasVelocityChangedEnough = false;

        pm.swinging = false;
        pm.currentState = PlayerController.MovementState.Grappling;

        if (hookJoints[0]) Destroy(hookJoints[0]);
        if (hookJoints[1]) Destroy(hookJoints[1]);

        hookJoints[0] = null;
        hookJoints[1] = null;

        rb.useGravity = false;

        grappleMidpoint = (hookWorldPoints[0] + hookWorldPoints[1]) * 0.5f;
        grappleStartPosition = player.position;
        hasPassedStartDistance = false;

        Vector3 toGrapple = (grappleMidpoint - player.position).normalized;
        Vector3 currentVelocity = rb.linearVelocity;

        float startDistance = Vector3.Distance(player.position, grappleMidpoint);
        float speedFromDistance = GrappleSpeed * (DistanceSpeedModifier * startDistance);

        // Momentum contribution if moving toward grapple
        float momentumContribution = 0f;
        float alignment = Vector3.Dot(currentVelocity.normalized, toGrapple);

        if (alignment > 0.1f)
        {
            momentumContribution = currentVelocity.magnitude * alignment;
        }

        FinalGrappleVelocity = momentumContribution + speedFromDistance;

        // Enforce min speed
        if (FinalGrappleVelocity < MinGrappleSpeed)
            FinalGrappleVelocity = MinGrappleSpeed;

        if (FinalGrappleVelocity > MaxGrappleSpeed)
            FinalGrappleVelocity = MaxGrappleSpeed;

        grappleSpeed = FinalGrappleVelocity;
        grappleInitialVelocity = currentVelocity;
        lastVelocitySample = currentVelocity;

        // Disable gravity during grapple
        rb.useGravity = false;

        fuel -= dualHookCost;
        fuelRechargeTimer = fuelRechargeDelay;
    }




    private void ApplyDualGrappleMovement()
    {
        grappleTimer += Time.deltaTime;

        Vector3 toGrapple = grappleMidpoint - player.position;
        float distance = toGrapple.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 grappleDir = toGrapple.normalized;

        // Check if we've passed start distance yet (elastic phase done)
        float startDistance = Vector3.Distance(grappleStartPosition, grappleMidpoint);
        if (!hasPassedStartDistance && distance < startDistance)
            hasPassedStartDistance = true;

        // Only start curve once elastic phase is done
        if (hasPassedStartDistance)
        {
            float t = Mathf.Clamp01(grappleTimer / MaxGrappleTime);
            float curve = grappleAccelerationCurve.Evaluate(t);

            float targetSpeed = Mathf.Lerp(
                rb.linearVelocity.magnitude,
                FinalGrappleVelocity,
                curve
            );

            Vector3 targetVelocity = grappleDir * targetSpeed;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * ReelInSpeed);
        }
        else
        {
            // Elastic pull phase (no curve yet)
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                grappleDir * FinalGrappleVelocity,
                Time.deltaTime * 3f
            );
        }

        // Release when close
        if (distance <= GrappleReleaseDistance)
        {
            FinishGrapple();
            return;
        }

        // Failsafe timeout
        if (grappleTimer >= MaxGrappleTime)
        {
            FinishGrapple();
            return;
        }

    }





    private void FinishGrapple()
    {
        StartCoroutine(RestoreGravityDelayed());
        dualGrappleActive = false;
        grappleVisualActive = false;

        pm.currentState = pm.isGrounded
            ? PlayerController.MovementState.Grounded
            : PlayerController.MovementState.Airborne;

        hookActive[0] = false;
        hookActive[1] = false;

        lineRenderers[0].positionCount = 0;
        lineRenderers[1].positionCount = 0;
    }

    private IEnumerator RestoreGravityDelayed()
    {
        yield return new WaitForSeconds(gravityRestoreDelay);
        rb.useGravity = true;
    }

    private void TryStartHook(int hookIndex)
    {
        if (predictedHits[hookIndex].collider == null) return;
        if (fuel < singleHookCost) return;

        hookActive[hookIndex] = true;
        hookWorldPoints[hookIndex] = predictedHits[hookIndex].point;

        bool otherHookActive = hookIndex == 0 ? hookActive[1] : hookActive[0];
        bool withinWindow = Mathf.Abs(lastLeftHookTime - lastRightHookTime) <= GrappleWindow;

        if (otherHookActive && withinWindow)
            return;

        pm.swinging = true;

        fuel -= singleHookCost;
        fuelRechargeTimer = fuelRechargeDelay;

        SpringJoint joint = player.gameObject.AddComponent<SpringJoint>();
        hookJoints[hookIndex] = joint;

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = hookWorldPoints[hookIndex];

        float distanceToHook = Vector3.Distance(player.position, hookWorldPoints[hookIndex]);
        joint.maxDistance = distanceToHook * swingDistanceRatio.y;
        joint.minDistance = distanceToHook * swingDistanceRatio.x;

        joint.spring = springForce;
        joint.damper = springDamping;
        joint.massScale = springMassScale;

        lineRenderers[hookIndex].positionCount = 2;
        ropeVisualPositions[hookIndex] = gunTips[hookIndex].position;
    }

    private void StopHook(int hookIndex)
    {
        if (dualGrappleActive)
            return;

        hookActive[hookIndex] = false;

        if (hookJoints[hookIndex] != null)
            Destroy(hookJoints[hookIndex]);

        if (!AnyHookActive())
            pm.swinging = false;
    }

    private bool AnyHookActive()
    {
        return hookActive[0] || hookActive[1];
    }

    private void UpdateRopeVisuals()
    {
        for (int i = 0; i < hookCount; i++)
        {
            // Grapple mode: straight visual rope
            if (grappleVisualActive)
            {
                lineRenderers[i].positionCount = 2;
                lineRenderers[i].SetPosition(0, gunTips[i].position);
                lineRenderers[i].SetPosition(1, hookWorldPoints[i]);
                continue;
            }

            // Swinging mode
            if (!hookActive[i])
            {
                lineRenderers[i].positionCount = 0;
                continue;
            }

            ropeVisualPositions[i] = Vector3.Lerp(
                ropeVisualPositions[i],
                hookWorldPoints[i],
                Time.deltaTime * 8f
            );

            lineRenderers[i].SetPosition(0, gunTips[i].position);
            lineRenderers[i].SetPosition(1, ropeVisualPositions[i]);
        }
    }


    private void UpdatePredictionPoints()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenCenterX = screenWidth * 0.5f;
        float screenCenterY = screenHeight * 0.5f;

        int samplesPerSide = rb.linearVelocity.magnitude > velocitySampleThreshold
            ? minSamplesPerSide
            : maxSamplesPerSide;
        float invSamples = 1f / samplesPerSide;

        // Check if one hook is already reachable to allow leniency for second hook
        bool firstHookReachable = false;
        for (int i = 0; i < hookCount; i++)
        {
            if (hookActive[i]) continue;
            if (predictedHits[i].collider != null && Vector3.Distance(player.position, predictedHits[i].point) <= maxSwingDistance)
            {
                firstHookReachable = true;
                break;
            }
        }

        for (int hookIndex = 0; hookIndex < hookCount; hookIndex++)
        {
            if (hookActive[hookIndex]) continue;

            float bestScore = float.MaxValue;
            RaycastHit bestHit = default;

            // Define left/right screen half
            float minX = hookIndex == 0 ? 0f : screenCenterX;
            float maxX = hookIndex == 0 ? screenCenterX : screenWidth;
            float halfCenterX = hookIndex == 0 ? screenCenterX * 0.5f : screenCenterX + screenCenterX * 0.5f;

            // Allow leniency for second hook
            float allowedDistance = maxSwingDistance + (firstHookReachable ? GrappleDistanceLeniency : 0f);

            // Sample across the screen half
            for (int s = 0; s < samplesPerSide; s++)
            {
                float t = Mathf.Pow((s + 0.5f) * invSamples, sideBiasStrictness);
                float screenX = Mathf.Lerp(minX, maxX, t);

                Ray ray = cachedCamera.ScreenPointToRay(new Vector3(screenX, screenCenterY, 0f));

                // Only hit grappleable objects
                if (!Physics.Raycast(ray, out RaycastHit hit, allowedDistance, whatIsGrappleable, QueryTriggerInteraction.Ignore))
                    continue;

                // Score based on distance, screen half bias, and edge penalty
                float distScore = hit.distance;
                float halfBias = Mathf.Abs(screenX - halfCenterX) / screenCenterX;
                float edgeDist = Mathf.Min(screenX, screenWidth - screenX) / screenCenterX;
                float edgePenalty = Mathf.Pow(1f - edgeDist, 3f) * 10f;

                float score = distScore + halfBias * sideBiasStrictness + edgePenalty;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestHit = hit;
                }
            }

            // Apply the best valid hit
            if (bestHit.collider != null)
            {
                predictionPoints[hookIndex].gameObject.SetActive(true);
                predictionPoints[hookIndex].position = bestHit.point;
                predictedHits[hookIndex] = bestHit;
            }
            else
            {
                predictionPoints[hookIndex].gameObject.SetActive(false);
                predictedHits[hookIndex] = default;
            }
        }
    }




    private void DisablePredictionPoints()
    {
        for (int i = 0; i < hookCount; i++)
        {
            if (predictionPoints[i] != null)
                predictionPoints[i].gameObject.SetActive(false);

            predictedHits[i] = default;
        }
    }
}
