using System.Collections;
using UnityEngine;

public class BugIdleAnimator : MonoBehaviour
{
    public enum IdleState
    {
        AimlessLook,
        LookAtPlayer,
        SillyMode
    }

    [Header("References")]
    public Transform head;
    public Transform legBL;
    public Transform legBR;
    public Transform legFR;
    public Transform legFL;
    public Transform player;

    [Header("State Settings")]
    public Vector2 aimlessLookInterval = new Vector2(2f, 5f);
    public Vector2 lookAtPlayerDuration = new Vector2(2f, 4f);
    public float headLerpSpeed = 3f;
    public AnimationCurve headRotateCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Silly Mode")]
    public float SpinSpeed = 200f;
    public float SpinTime = 3f;
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Leg Stomp")]
    public float stompAngle = 33f;
    public float stompSpeed = 1f;
    public float stompTime = 0.25f;
    public AnimationCurve stompCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Idle Breathing")]
    public Transform body;
    public float breathHeight = 0.15f;
    public float breathSpeed = 1.5f;
    public float breathLegLift = 12f;
    public AnimationCurve breathCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private IdleState currentState;
    private IdleState lastState;

    private Vector3 bodyStartLocalPos;

    private Coroutine stateRoutine;

    private void Start()
    {
        bodyStartLocalPos = body.localPosition;

        SwitchToRandomState();
    }

    private void SwitchToRandomState()
    {
        IdleState nextState;

        do
        {
            nextState = (IdleState)Random.Range(0, 3);
        }
        while (nextState == lastState);

        lastState = currentState;
        currentState = nextState;

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        stateRoutine = StartCoroutine(RunState(currentState));
    }

    private IEnumerator RunState(IdleState state)
    {
        switch (state)
        {
            case IdleState.AimlessLook:
                yield return StartCoroutine(AimlessLookRoutine());
                break;

            case IdleState.LookAtPlayer:
                yield return StartCoroutine(LookAtPlayerRoutine());
                break;

            case IdleState.SillyMode:
                yield return StartCoroutine(SillyModeRoutine());
                break;
        }

        if (currentState == IdleState.AimlessLook || currentState == IdleState.LookAtPlayer)
            ApplyIdleBreathing();

        yield return StartCoroutine(ResetHeadRotation());
        yield return new WaitForSeconds(0.2f);
        SwitchToRandomState();
    }

    // -----------------------------------------------------
    // Aimless Look
    // -----------------------------------------------------

    private IEnumerator AimlessLookRoutine()
    {
        float duration = Random.Range(aimlessLookInterval.x, aimlessLookInterval.y);
        float timer = 0f;

        float startY = head.localEulerAngles.y;
        float targetY;
        do
        {
            targetY = Random.Range(0f, 360f);
        }
        while (Mathf.Abs(Mathf.DeltaAngle(startY, targetY)) < 60f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float curveT = headRotateCurve.Evaluate(t);

            float newY = Mathf.LerpAngle(startY, targetY, curveT);
            Vector3 rot = head.localEulerAngles;
            rot.y = Mathf.LerpAngle(rot.y, newY, Time.deltaTime * headLerpSpeed);
            head.localEulerAngles = rot;

            yield return null;
        }
    }

    // -----------------------------------------------------
    // Look At Player
    // -----------------------------------------------------

    private IEnumerator LookAtPlayerRoutine()
    {
        float duration = Random.Range(lookAtPlayerDuration.x, lookAtPlayerDuration.y);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            Vector3 dir = player.position - head.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                Vector3 rot = head.rotation.eulerAngles;
                rot.y = Quaternion.Slerp(
                    Quaternion.Euler(0f, rot.y, 0f),
                    Quaternion.Euler(0f, targetRot.eulerAngles.y, 0f),
                    Time.deltaTime * headLerpSpeed
                ).eulerAngles.y;

                head.rotation = Quaternion.Euler(0f, rot.y, 0f);
            }

            yield return null;
        }
    }

    // -----------------------------------------------------
    // Silly Mode
    // -----------------------------------------------------

    private IEnumerator SillyModeRoutine()
    {
        float timer = 0f;
        float spinTimer = 0f;

        Quaternion startRot = head.localRotation;

        // Run 2 full stomp cycles
        for (int loop = 0; loop < 2; loop++)
        {
            yield return StartCoroutine(StompLeg(legFL));
            yield return StartCoroutine(StompLeg(legFR));
            yield return StartCoroutine(StompLeg(legBR));
            yield return StartCoroutine(StompLeg(legBL));
        }

        // Head spin during stomping
        while (spinTimer < SpinTime)
        {
            spinTimer += Time.deltaTime;
            float t = spinTimer / SpinTime;
            float curveT = spinCurve.Evaluate(t);

            float spinAmount = SpinSpeed * curveT * Time.deltaTime;
            head.Rotate(Vector3.up, spinAmount, Space.Self);

            yield return null;
        }

        // Smooth stop back to zero
        yield return StartCoroutine(ResetHeadRotation());
    }


    // -----------------------------------------------------
    // Stompy stomps :)
    // -----------------------------------------------------

    private IEnumerator LegStompCycle()
    {
        while (true)
        {
            yield return StartCoroutine(StompLeg(legFL));
            yield return StartCoroutine(StompLeg(legFR));
            yield return StartCoroutine(StompLeg(legBR));
            yield return StartCoroutine(StompLeg(legBL));
        }
    }

    private IEnumerator StompLeg(Transform leg)
    {
        ResetLegs();

        float timer = 0f;
        float duration = 1f / stompSpeed;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float curveT = stompCurve.Evaluate(t);

            SetLegX(leg, stompAngle * curveT);
            yield return null;
        }

        SetLegX(leg, 0f);

        yield return new WaitForSeconds(stompTime);
    }


    private void ResetLegs()
    {
        SetLegX(legFL, 0f);
        SetLegX(legFR, 0f);
        SetLegX(legBL, 0f);
        SetLegX(legBR, 0f);
    }

    private void ResetLeg(Transform leg)
    {
        Vector3 rot = leg.localEulerAngles;
        rot.x = 0f;
        leg.localEulerAngles = rot;
    }

    private IEnumerator ResetHeadRotation()
    {
        float timer = 0f;
        float duration = 0.35f;

        Quaternion startRot = head.localRotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float curveT = headRotateCurve.Evaluate(t);

            head.localRotation = Quaternion.Slerp(startRot, targetRot, curveT);
            yield return null;
        }

        head.localRotation = targetRot;
    }

    private void ApplyIdleBreathing()
    {
        float t = Mathf.PingPong(Time.time * breathSpeed, 1f);
        float curveT = breathCurve.Evaluate(t);

        // Body down/up
        body.localPosition = bodyStartLocalPos + Vector3.down * breathHeight * curveT;

        // Legs lift slightly together
        float legAngle = breathLegLift * curveT;
        SetLegX(legFL, legAngle);
        SetLegX(legFR, legAngle);
        SetLegX(legBL, legAngle);
        SetLegX(legBR, legAngle);
    }

    private void SetLegX(Transform leg, float x)
    {
        Vector3 rot = leg.localEulerAngles;
        rot.x = x;
        leg.localEulerAngles = rot;
    }

}
