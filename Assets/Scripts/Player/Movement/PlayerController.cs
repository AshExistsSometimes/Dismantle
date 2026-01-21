using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    #region Debug
    [Header("Debug")]
    public float PlayerVelocity;
    [SerializeField] public bool isGrounded;
    #endregion

    #region References
    [Header("References")]
    public Transform orientation;
    public Transform headPosition;
    public Transform cameraHolder;
    #endregion

    #region Movement
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float groundAcceleration = 60f;
    public float groundDragRate = 25f;
    public float airAcceleration = 20f;
    public float airDragRate = 5f;
    public float jumpForce = 4f;
    [Space]
    public AnimationCurve horizontalDragCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    public float dragTime = 0.2f; // for low-speed stop
    #endregion

    #region Slide
    [Header("Slide")]
    public float slideSpeed = 6f;
    public float maxSlideSpeed = 20f;
    public float SlideJumpModifier = 1.2f;
    public float slideSpeedBoost = 8f;

    public float slideDuration = 0.75f;

    public float slideScaleY = 0.5f;
    #endregion

    #region Air Slide
    [Header("Air Slide")]
    public float airSlideForwardForce = 8f;
    public float airSlideDownForce = 6f;
    #endregion

    #region Wallrunning
    [Header("Wallrunning")]
    public LayerMask WallLayers;
    public LayerMask GroundLayers;

    public float WallRunForce = 2f;
    public float MaxWallRunTime = 3f;
    public float MaxWallRunSpeed = 18f;
    public float wallJumpUpForce = 3f;
    public float wallJumpSideForce = 5f;
    public float WallCheckDistance = 0.75f;
    public float minJumpHeight = 1f;
    public float ExitWallTime = 0.2f;
    [Space]
    public AnimationCurve wallRunDragCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [Space]
    public bool movementLocked;

    private Vector3 preWallrunHorizontalVelocity;
    private bool wallRunTimerStarted;
    #endregion

    #region Swinging
    [Header("Swinging")]
    public bool swinging;
    #endregion

    #region Ground Check
    [Header("Ground Check")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;
    #endregion

    #region Gravity
    [Header("Gravity")]
    public float GravityModifier = 0f;
    public float MaxFallSpeed = 40f;
    #endregion

    #region Camera
    [Header("Mouse Look")]
    public Camera playerCam;

    public float mouseSensitivity = 100f;
    public float maxLookAngle = 85f;

    [Header("Camera Effects")]
    public float SlideFOV = 80f;
    public float SlideFOVSpeed = 10f;

    public float WallRunFOV = 85f;
    public float WallRunFOVSpeed = 10f;

    public float WallRunCamAngle = 10f;
    public float WallRunCamAngleSpeed = 5f;

    private float originalFOV;
    private float targetFOV;
    private float targetZRotation;

    public bool CamEffectsEnabled = true;

    private float currentZRotation;
    #endregion

    #region Headbob
    [Header("Headbob")]
    public bool HeadBobEnabled = true;
    public float HeadbobSpeed = 1.5f;
    public float HeadbobAmount = 0.08f;
    public float HeadbobReturnSpeed = 8f;
    public float HeadbobBlendSpeed = 6f;
    public AnimationCurve HeadbobCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float headbobTimer;
    private float currentHeadbobOffset;
    private Vector3 defaultCamLocalPos;
    private float headbobBlend;
    #endregion

    #region State Machine
    public enum MovementState
    {
        Grounded,
        Airborne,
        Sliding,
        WallRunning,
        Swinging,
        Grappling,
        Locked
    }

    public MovementState currentState;
    #endregion

    #region Private Fields
    private Rigidbody rb;

    private float xRotation;
    private float horizontalInput;
    private float verticalInput;

    private bool slideHeld;
    private bool canAirSlide = true;
    private float slideTimer;
    private Vector3 slideDirection;
    private Vector3 originalScale;

    private bool wallRunning;
    private float wallRunTimer;
    private float exitWallTimer;
    private bool canWallRun = true;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    #endregion

    #region Unity Default

    private void Awake()
    {
        if (SettingsManager.Instance !=null)
        {
            mouseSensitivity = SettingsManager.Instance.MouseSensitivity;
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        originalScale = transform.localScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCam != null)
        {
            defaultCamLocalPos = playerCam.transform.localPosition;
            originalFOV = playerCam.fieldOfView;
        }

        targetFOV = originalFOV;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleInput();
        GroundCheck();
        HandleJumpInput();
        HandleSlideInput();

        CheckForWall();
        HandleWallrunning();

        UpdateStateMachine();

        UpdateCameraPosition();
    }

    private void FixedUpdate()
    {
        if (currentState == MovementState.WallRunning)
        {
            // Preserve horizontal velocity before collisions
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            preWallrunHorizontalVelocity = horizontalVelocity;
        }

        HandleMovement();
        HandleSlideTimer();
        ApplyCustomGravity();

        if (currentState == MovementState.WallRunning)
        {
            // Wallrunning: tilt and FOV
            WallRunMovement();
            DoFov(WallRunFOV, WallRunFOVSpeed);
            if (wallRight)
                DoCamTilt(WallRunCamAngle, WallRunCamAngleSpeed);
            else
                DoCamTilt(-WallRunCamAngle, WallRunCamAngleSpeed);
        }
        else
        {
            // Not wallrunning
            ResetCamTilt(WallRunCamAngleSpeed); // always reset tilt

            if (currentState != MovementState.Sliding && currentState != MovementState.Grappling)
                ResetFov(WallRunFOVSpeed); // reset FOV only if NOT sliding
        }

        UpdateVelocityDisplay();
        RestoreScaleIfPossible();
    }

    private void LateUpdate()
    {
        if (CamEffectsEnabled)
            UpdateCameraEffects();

        UpdateHeadbob();
    }

    #endregion

    #region Input

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.rotation = Quaternion.Euler(xRotation, orientation.eulerAngles.y, 0f);
        orientation.Rotate(Vector3.up * mouseX);
    }

    private void HandleInput()
    {
        if (currentState == MovementState.Grappling)
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    #endregion

    #region Ground Check

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        if (isGrounded)
            canAirSlide = true;
    }

    #endregion

    #region State Machine Controller

    private void UpdateStateMachine()
    {
        if (currentState == MovementState.Locked)
        {
            UpdateWallCooldown();
            return;
        }

        if (swinging)
        {
            currentState = MovementState.Swinging;
            return;
        }

        if (wallRunning)
        {
            currentState = MovementState.WallRunning;
            return;
        }

        if (slideTimer > 0f)
        {
            currentState = MovementState.Sliding;
            return;
        }

        if (isGrounded)
            currentState = MovementState.Grounded;
        else
            currentState = MovementState.Airborne;

        if (currentState == MovementState.WallRunning && Input.GetKeyDown(KeyCode.Space))
        {
            WallJump();
        }
    }

    #endregion

    #region Movement

    private void HandleMovement()
    {
        if (currentState == MovementState.Sliding ||
            currentState == MovementState.WallRunning ||
            currentState == MovementState.Swinging ||
            currentState == MovementState.Locked ||
            currentState == MovementState.Grappling)
            return;

        Vector3 inputDir = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Extract horizontal velocity
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        // Determine target speed
        Vector3 wishVelocity = inputDir * moveSpeed;
        float inputMagnitude = inputDir.sqrMagnitude;

        if (speed <= moveSpeed * 1.5f)
        {
            // Snappy stop if low speed
            Vector3 target = inputMagnitude > 0.01f ? wishVelocity : Vector3.zero;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, target, Time.fixedDeltaTime / dragTime);
        }
        else
        {
            // High-speed decay using curve
            float t = Mathf.Clamp01(Time.fixedDeltaTime / dragTime); // normalized delta
            float curveValue = horizontalDragCurve.Evaluate(t);     // 1 = current, 0 = target
            Vector3 target = inputMagnitude > 0.01f ? wishVelocity : Vector3.zero;
            horizontalVelocity = Vector3.Lerp(target, horizontalVelocity, curveValue);
        }

        // Apply horizontal velocity
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }


    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !swinging)
        {
            float finalJumpForce = jumpForce;

            // Slide jump boost
            if (currentState == MovementState.Sliding)
                finalJumpForce *= SlideJumpModifier;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * finalJumpForce, ForceMode.Impulse);
        }
    }

    #endregion

    #region Wallrunning

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, WallCheckDistance, WallLayers);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, WallCheckDistance, WallLayers);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, GroundLayers);
    }

    private void WallStateMachine()
    {
        bool touchingWall = (wallLeft || wallRight);

        if (touchingWall && verticalInput > 0 && AboveGround() && canWallRun)
        {
            if (!wallRunning)
                StartWallrun();

            if (wallRunTimer > 0f)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0f && wallRunning)
                BeginExitWall();

            if (Input.GetKeyDown(KeyCode.Space))
                WallJump();
        }
        else
        {
            if (wallRunning)
                StopWallrun();
        }

        if (!canWallRun)
        {
            if (exitWallTimer > 0f)
                exitWallTimer -= Time.deltaTime;
            else
            {
                canWallRun = true;
                movementLocked = false;
            }
        }
    }

    private void HandleWallrunning()
    {
        rb.useGravity = !wallRunning;

        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, WallCheckDistance, WallLayers);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, WallCheckDistance, WallLayers);

        bool touchingWall = wallLeft || wallRight;

        if (touchingWall && verticalInput > 0 && AboveGround() && canWallRun)
        {
            if (!wallRunning)
            {
                wallRunning = true;
                wallRunTimerStarted = false; // reset timer start
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }

            // Only start wallrun timer once horizontal speed <= MaxWallRunSpeed
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float speedAlongWall = horizontalVelocity.magnitude;

            if (!wallRunTimerStarted && speedAlongWall <= MaxWallRunSpeed)
            {
                wallRunTimerStarted = true;
                wallRunTimer = MaxWallRunTime;
            }

            // Countdown timer only if started
            if (wallRunTimerStarted)
            {
                if (wallRunTimer > 0f)
                    wallRunTimer -= Time.deltaTime;
                else
                {
                    wallRunning = false;
                    canWallRun = false;
                    exitWallTimer = ExitWallTime;
                }
            }

            // Jump from wall
            if (Input.GetKeyDown(KeyCode.Space))
                WallJump();
        }
        else
        {
            // Not touching wall
            if (wallRunning)
                wallRunning = false;
        }

        // Wall exit cooldown
        if (!canWallRun)
        {
            if (exitWallTimer > 0f)
                exitWallTimer -= Time.deltaTime;
            else
                canWallRun = true;
        }
    }

    private void StartWallrun()
    {
        wallRunning = true;
        canAirSlide = true;
        wallRunTimer = MaxWallRunTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(wallForward, orientation.forward) < 0)
            wallForward = -wallForward;

        // PRESERVE CURRENT VELOCITY
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Keep the portion along the wall
        float alongWall = Vector3.Dot(horizontalVelocity, wallForward);
        Vector3 preservedVelocity = wallForward * alongWall;

        // Keep any small into-wall velocity if moving away from wall
        float intoWall = Vector3.Dot(horizontalVelocity, wallNormal);
        if (intoWall < 0f)
            preservedVelocity += wallNormal * intoWall;

        // ONLY ZERO THE Y, keep your full XZ speed
        rb.linearVelocity = new Vector3(preservedVelocity.x, 0f, preservedVelocity.z);

        // Store this to continue adding forces each frame
        preWallrunHorizontalVelocity = preservedVelocity;
    }


    private void WallRunMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);

        if (Vector3.Dot(wallForward, orientation.forward) < 0)
            wallForward = -wallForward;

        // Project current horizontal velocity along the wall
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float alongWall = Vector3.Dot(horizontalVelocity, wallForward);
        float intoWall = Vector3.Dot(horizontalVelocity, wallNormal);

        // Compute target velocity along the wall
        float targetAlongWall = MaxWallRunSpeed;

        // Interpolate safely using MoveTowards (no overshoot)
        float t = wallRunDragCurve.Evaluate(1f - (wallRunTimer / MaxWallRunTime));
        float maxDelta = t * 10f * Time.fixedDeltaTime; // adjust speed toward target
        alongWall = Mathf.MoveTowards(alongWall, targetAlongWall, maxDelta);

        // Rebuild velocity
        Vector3 newVelocity = wallForward * alongWall + wallNormal * Mathf.Min(intoWall, 0f);

        // Apply horizontal velocity; vertical = 0 for flat plane
        rb.linearVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);
    }




    private void BeginExitWall()
    {
        StopWallrun();

        ResetFov(WallRunFOVSpeed);
        ResetCamTilt(WallRunCamAngleSpeed);

        canWallRun = false;
        movementLocked = true;
        exitWallTimer = ExitWallTime;
    }

    private void StopWallrun()
    {
        wallRunning = false;
        rb.useGravity = true;
    }

    private void WallJump()
    {
        BeginExitWall();

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }


    private void UpdateWallCooldown()
    {
        if (exitWallTimer > 0f)
        {
            exitWallTimer -= Time.deltaTime;
        }
        else
        {
            movementLocked = false;
            canWallRun = true;
            currentState = isGrounded ? MovementState.Grounded : MovementState.Airborne;
        }
    }

    #endregion



    #region Slide

    private void HandleSlideInput()
    {
        slideHeld = Input.GetKey(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isGrounded || canAirSlide)
                StartSlide();
        }

        if (slideTimer > 0f && !slideHeld)
            EndSlide();
    }

    private void HandleSlideTimer()
    {
        if (slideTimer <= 0f)
            return;

        slideTimer -= Time.fixedDeltaTime;

        if (slideTimer <= 0f)
            EndSlide();
    }

    private void StartSlide()
    {
        // Consume air slide
        if (!isGrounded)
            canAirSlide = false;

        slideTimer = slideDuration;
        currentState = MovementState.Sliding;

        DoFov(SlideFOV, SlideFOVSpeed);

        // Get directional input
        Vector3 inputDir = (orientation.forward * verticalInput + orientation.right * horizontalInput);
        inputDir.y = 0f;

        // If theres an input, use that. otherwise use momentum direction
        if (inputDir.sqrMagnitude > 0.01f)
            slideDirection = inputDir.normalized;
        else
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            slideDirection = horizontalVelocity.sqrMagnitude > 0.01f
                ? horizontalVelocity.normalized
                : orientation.forward;
        }

        // Apply slide velocity
        float currentSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
        float speed = Mathf.Clamp(currentSpeed + slideSpeedBoost, slideSpeed, maxSlideSpeed);

        rb.linearVelocity = slideDirection * speed + Vector3.up * rb.linearVelocity.y;

        // Snap to ground if grounded
        if (isGrounded)
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // Scale player
        transform.localScale = new Vector3(originalScale.x, originalScale.y * slideScaleY, originalScale.z);
    }

    private void EndSlide()
    {
        ResetFov(SlideFOVSpeed);
        slideTimer = 0f;
        RestoreScaleIfPossible();
    }

    #endregion

    #region Gravity

    private void ApplyCustomGravity()
    {
        if (GravityModifier == 0f || isGrounded || currentState == MovementState.WallRunning)
            return;

        rb.AddForce(Vector3.down * GravityModifier, ForceMode.Acceleration);

        if (rb.linearVelocity.y < -MaxFallSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -MaxFallSpeed, rb.linearVelocity.z);
        }
    }

    #endregion

    #region Camera

    private void UpdateCameraPosition()
    {
        cameraHolder.position = headPosition.position;
    }

    // Smoothly lerp the camera FOV to newValue
    public void DoFov(float newValue, float lerpSpeed)
    {
        targetFOV = newValue;

        if (playerCam != null)
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, lerpSpeed * Time.deltaTime);
    }

    // Smoothly reset FOV to original
    public void ResetFov(float lerpSpeed)
    {
        targetFOV = originalFOV;

        if (playerCam != null)
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, lerpSpeed * Time.deltaTime);
    }

    // Smoothly tilt the camera (z rotation)
    public void DoCamTilt(float zTilt, float lerpSpeed)
    {
        targetZRotation = zTilt;

        if (playerCam != null)
        {
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, lerpSpeed * Time.deltaTime);
            Vector3 rot = playerCam.transform.localEulerAngles;
            rot.z = currentZRotation;
            playerCam.transform.localEulerAngles = rot;
        }
    }

    // Smoothly reset camera tilt
    public void ResetCamTilt(float lerpSpeed)
    {
        targetZRotation = 0f;

        if (playerCam != null)
        {
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, lerpSpeed * Time.deltaTime);
            Vector3 rot = playerCam.transform.localEulerAngles;
            rot.z = currentZRotation;
            playerCam.transform.localEulerAngles = rot;
        }
    }

    private void UpdateCameraEffects()
    {
        if (playerCam == null) return;

        // Lerp FOV
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, 5f * Time.deltaTime);

        // Lerp Z tilt
        currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, 5f * Time.deltaTime);
        Vector3 rot = playerCam.transform.localEulerAngles;
        rot.z = currentZRotation;
        playerCam.transform.localEulerAngles = rot;
    }

    private void UpdateHeadbob()
    {
        if (!HeadBobEnabled || playerCam == null)
            return;

        bool hasMovementInput = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;

        bool shouldBob =
            (hasMovementInput && isGrounded && currentState == MovementState.Grounded) ||
            currentState == MovementState.WallRunning;

        // Blend bob strength in/out
        float targetBlend = shouldBob ? 1f : 0f;
        headbobBlend = Mathf.Lerp(headbobBlend, targetBlend, HeadbobBlendSpeed * Time.deltaTime);

        if (headbobBlend > 0.001f)
        {
            // Advance curve time
            headbobTimer += Time.deltaTime * HeadbobSpeed;

            if (headbobTimer > 1f)
                headbobTimer -= 1f;

            float curveValue = HeadbobCurve.Evaluate(headbobTimer);
            float centered = (curveValue - 0.5f) * 2f;

            currentHeadbobOffset = centered * HeadbobAmount * headbobBlend;
        }
        else
        {
            headbobTimer = 0f;
            currentHeadbobOffset = 0f;
        }

        Vector3 camPos = defaultCamLocalPos;
        camPos.y += currentHeadbobOffset;
        playerCam.transform.localPosition = camPos;
    }



    #endregion

    #region Utilities

    private void RestoreScaleIfPossible()
    {
        // Only restore if not sliding
        if (slideTimer > 0f)
            return;

        // Calculate the top of the player's collider
        float playerHeight = originalScale.y;
        Vector3 topOfPlayer = transform.position + Vector3.up * (playerHeight * 0.5f);

        // Check if there is enough space above for full height
        float spaceCheckDistance = playerHeight * 0.5f;
        bool spaceAbove = !Physics.Raycast(topOfPlayer, Vector3.up, spaceCheckDistance, groundMask | WallLayers);

        if (spaceAbove && transform.localScale.y != originalScale.y)
            transform.localScale = originalScale;
    }


    public void UpdateVelocityDisplay()
    {
        PlayerVelocity = rb.linearVelocity.magnitude;
    }

    #endregion
}
