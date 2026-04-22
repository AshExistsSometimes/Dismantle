using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour
{
    [System.Serializable]
    public class PointData
    {
        public Transform point;
        public float setSpeed = 5f;      // How fast to move to this point
        public float pauseTime = 0f;     // How long to pause at this point
    }

    [Header("Point Settings")]
    public PointData[] points;

    [Header("Movement Settings")]
    public bool loop = true;
    public bool startOnTrigger = false;
    public bool stopOnEnd = false;

    private int currentIndex = 0;
    private bool goingForward = true;
    private float t = 0f;
    private bool isMoving = true;
    private bool isPaused = false;
    private bool waitingForTrigger = false;

    void Start()
    {
        // Snap to the first point at start
        if (points != null && points.Length > 0 && points[0].point != null)
            transform.position = points[0].point.position;

        // If StartOnTrigger is enabled, movement won’t start until Activate() is called
        if (startOnTrigger)
        {
            isMoving = false;
            waitingForTrigger = true;
        }
    }

    void Update()
    {
        if (!isMoving || isPaused) return;
        if (points == null || points.Length == 0) return;

        if (points.Length == 1)
        {
            transform.position = points[0].point.position;
            return;
        }

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        Transform startPoint = points[currentIndex].point;
        int nextIndex;

        // Determine next point index
        if (goingForward)
        {
            nextIndex = currentIndex + 1;
            if (nextIndex >= points.Length)
            {
                if (stopOnEnd)
                {
                    // If we stop on end, halt completely
                    isMoving = false;
                    return;
                }

                if (loop)
                {
                    nextIndex = 0;
                }
                else
                {
                    goingForward = false;
                    nextIndex = currentIndex - 1;
                }
            }
        }
        else
        {
            nextIndex = currentIndex - 1;
            if (nextIndex < 0)
            {
                if (stopOnEnd)
                {
                    isMoving = false;
                    return;
                }

                goingForward = true;
                nextIndex = currentIndex + 1;
            }
        }

        Transform nextPoint = points[nextIndex].point;

        // Movement speed for THIS leg of the journey
        float speed = points[currentIndex].setSpeed;

        // Lerp step
        t += Time.deltaTime * speed / Vector3.Distance(startPoint.position, nextPoint.position);
        transform.position = Vector3.Lerp(startPoint.position, nextPoint.position, t);

        // If we reached the next point
        if (t >= 1f)
        {
            t = 0f;
            currentIndex = nextIndex;

            // Pause if this point has a pauseTime
            if (points[currentIndex].pauseTime > 0f)
            {
                StartCoroutine(HandlePause(points[currentIndex].pauseTime));
            }
        }
    }

    private IEnumerator HandlePause(float pauseDuration)
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }

    /// <summary>
    /// Call this to start or resume movement if StartOnTrigger is enabled.
    /// </summary>
    public void Activate()
    {
        isMoving = true;
        waitingForTrigger = false;
    }

    /// <summary>
    /// Call this to manually pause movement.
    /// </summary>
    public void Pause()
    {
        isMoving = false;
    }

    /// <summary>
    /// Call this to manually resume movement (only works if not waiting for trigger).
    /// </summary>
    public void Resume()
    {
        if (!waitingForTrigger)
            isMoving = true;
    }
}
