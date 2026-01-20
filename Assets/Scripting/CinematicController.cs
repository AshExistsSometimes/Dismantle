using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class CinematicStep
{
    public List<CinematicCameraStop> CameraStops = new();
    public UnityEvent OnStepStarted;
}

[System.Serializable]
public class CinematicCameraStop
{
    public Transform Transform;
    public float SegmentDuration = 2f;
    public AnimationCurve MoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}

public class CinematicController : MonoBehaviour
{
    [Header("Global Control")]
    public bool CinematicsDisabled = false;

    [Header("Disable during cinematic")]
    public List<GameObject> DisableDuringCinematic;

    [Header("References")]
    public GameObject Player;
    public Transform CameraHolder;

    [Header("Cinematic Steps")]
    public List<CinematicStep> Steps = new();

    [Header("Skip Settings")]
    public KeyCode SkipKey = KeyCode.Escape;
    public float SkipHoldTime = 1.5f;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private bool cinematicRunning = false;
    private float skipTimer = 0f;
    private Coroutine cinematicRoutine;

    public void StartCinematic()
    {
        if (CinematicsDisabled || cinematicRunning || Steps.Count == 0)
            return;

        foreach (GameObject obj in DisableDuringCinematic) // Disable all gameobjects in list (IE: HUD, Held Items)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        cinematicRoutine = StartCoroutine(RunCinematic());
    }

    private IEnumerator RunCinematic()
    {
        cinematicRunning = true;

        originalCameraPosition = CameraHolder.position;
        originalCameraRotation = CameraHolder.rotation;

        if (Player != null)
            Player.SetActive(false);

        for (int stepIndex = 0; stepIndex < Steps.Count; stepIndex++)
        {
            CinematicStep step = Steps[stepIndex];

            step.OnStepStarted?.Invoke();

            if (step.CameraStops.Count == 0)
                continue;

            // Move from current camera to first stop
            yield return StartCoroutine(
                MoveCameraSegment(
                    new CinematicCameraStop
                    {
                        Transform = CameraHolder,
                        SegmentDuration = step.CameraStops[0].SegmentDuration,
                        MoveCurve = step.CameraStops[0].MoveCurve
                    },
                    step.CameraStops[0]
                )
            );

            // 2Move through remaining stops
            for (int stopIndex = 0; stopIndex < step.CameraStops.Count - 1; stopIndex++)
            {
                CinematicCameraStop from = step.CameraStops[stopIndex];
                CinematicCameraStop to = step.CameraStops[stopIndex + 1];

                yield return StartCoroutine(MoveCameraSegment(from, to));

                if (!cinematicRunning)
                    yield break;
            }

            // Teleport to next step's first stop (if any)
            if (stepIndex + 1 < Steps.Count &&
                Steps[stepIndex + 1].CameraStops.Count > 0)
            {
                Transform nextStart = Steps[stepIndex + 1].CameraStops[0].Transform;
                CameraHolder.position = nextStart.position;
                CameraHolder.rotation = nextStart.rotation;
            }
        }

        EndCinematic();
    }


    private IEnumerator MoveCameraSegment(
        CinematicCameraStop from,
        CinematicCameraStop to
    )
    {
        float elapsed = 0f;

        while (elapsed < from.SegmentDuration)
        {
            HandleSkipInput();

            if (!cinematicRunning)
                yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / from.SegmentDuration);
            float easedT = from.MoveCurve.Evaluate(t);

            CameraHolder.position = Vector3.Lerp(
                from.Transform.position,
                to.Transform.position,
                easedT
            );

            CameraHolder.rotation = Quaternion.Slerp(
                from.Transform.rotation,
                to.Transform.rotation,
                easedT
            );

            yield return null;
        }
    }

    private void HandleSkipInput()
    {
        if (Input.GetKey(SkipKey))
        {
            skipTimer += Time.deltaTime;

            if (skipTimer >= SkipHoldTime)
            {
                StopCinematicImmediate();
            }
        }
        else
        {
            skipTimer = 0f;
        }
    }

    private void StopCinematicImmediate()
    {
        if (!cinematicRunning)
            return;

        if (cinematicRoutine != null)
            StopCoroutine(cinematicRoutine);

        EndCinematic();
    }

    private void EndCinematic()
    {
        cinematicRunning = false;
        skipTimer = 0f;

        CameraHolder.position = originalCameraPosition;
        CameraHolder.rotation = originalCameraRotation;

        foreach (GameObject obj in DisableDuringCinematic)// Enable all gameobjects in list again
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (Player != null)
            Player.SetActive(true);
    }
}
