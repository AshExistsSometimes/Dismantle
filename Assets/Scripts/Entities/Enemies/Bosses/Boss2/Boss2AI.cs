using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2AI : MonoBehaviour, IDamagable
{
    [Header("References")]
    public Transform BossPivotPoint;

    [Header("State")]
    public bool BossActive = false;

    [Header("Health")]
    public int MaxHealth = 200;
    public int currentHealth;

    [Header("Movement Area")]
    [Range(0f, 90f)]
    public float MinPitch = 85f; // Lowest allowed point on sphere

    [Header("Movement")]
    public Vector2 BossStopTime = new Vector2(0.5f, 1.5f);
    public float MoveDuration = 2f; // Base travel time
    public float MinTravelDistance = 20f; // degrees on the sphere surface
    public AnimationCurve MoveSpeed;

    [Header("Path")]
    public int PathPoints = 3; // how many winding points between start and end
    public float BezierStrength = 15f; // how far it can curve off straight path

    [Header("Debug")]
    public bool DrawGizmos = true;
    public float GizmoSphereSize = 0.3f;
    public float GizmoDistanceFromCentre = 5f;
    public int BezierDetail = 24;

    private Coroutine movementRoutine;

    private Vector2 currentRot; // x = pitch, y = yaw
    private Vector2 targetRot;

    private List<Vector2> pathPoints = new List<Vector2>();

    private void Awake()
    {
        currentHealth = MaxHealth;
        currentRot = Vector2.zero;
        BossPivotPoint.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void Update()
    {
        if (BossActive && movementRoutine == null)
        {
            movementRoutine = StartCoroutine(MovementLoop());
        }
    }

    // -------------------------------
    // CORE LOOP
    // -------------------------------
    IEnumerator MovementLoop()
    {
        while (BossActive)
        {
            GenerateNewPath();
            yield return MoveAlongPath();

            float stopTime = Random.Range(BossStopTime.x, BossStopTime.y);
            yield return new WaitForSeconds(stopTime);
        }

        movementRoutine = null;
    }

    // -------------------------------
    // PATH GENERATION
    // -------------------------------
    void GenerateNewPath()
    {
        pathPoints.Clear();

        Vector2 start = currentRot;
        Vector2 end = GetRandomRotation();

        pathPoints.Add(start);

        for (int i = 0; i < PathPoints; i++)
        {
            float t = (i + 1f) / (PathPoints + 1f);
            Vector2 mid = Vector2.Lerp(start, end, t);

            mid.x += Random.Range(-BezierStrength, BezierStrength);
            mid.y += Random.Range(-BezierStrength, BezierStrength);

            mid.x = Mathf.Clamp(mid.x, 0f, MinPitch);
            mid.y = Mathf.Repeat(mid.y, 360f);

            pathPoints.Add(mid);
        }

        pathPoints.Add(end);
    }

    Vector2 GetRandomRotation()
    {
        Vector2 candidate;
        float arcDistance;

        float radius = transform.localPosition.magnitude;

        do
        {
            float pitch = Random.Range(0f, MinPitch);
            float yaw = Random.Range(0f, 360f);
            candidate = new Vector2(pitch, yaw);

            Vector3 fromDir = RotationToDirection(currentRot);
            Vector3 toDir = RotationToDirection(candidate);

            float angleRad = Vector3.Angle(fromDir, toDir) * Mathf.Deg2Rad;
            arcDistance = angleRad * radius;

        } while (arcDistance < MinTravelDistance);

        return candidate;
    }
    Vector3 RotationToDirection(Vector2 rot)
    {
        return Quaternion.Euler(rot.x, rot.y, 0f) * Vector3.up;
    }

    // -------------------------------
    // MOVEMENT
    // -------------------------------
    IEnumerator MoveAlongPath()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / MoveDuration;
            float eased = MoveSpeed.Evaluate(t);

            Vector2 rot = EvaluateBezier(pathPoints, eased);
            ApplyRotation(rot);

            yield return null;
        }

        currentRot = pathPoints[pathPoints.Count - 1];
    }

    void ApplyRotation(Vector2 rot)
    {
        BossPivotPoint.localRotation = Quaternion.Euler(rot.x, rot.y, 0f);
    }

    Vector2 EvaluateBezier(List<Vector2> points, float t)
    {
        List<Vector2> temp = new List<Vector2>(points);

        while (temp.Count > 1)
        {
            for (int i = 0; i < temp.Count - 1; i++)
            {
                temp[i] = Vector2.Lerp(temp[i], temp[i + 1], t);
            }
            temp.RemoveAt(temp.Count - 1);
        }

        return temp[0];
    }

    // -------------------------------
    // DAMAGE SYSTEM
    // -------------------------------
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        BossActive = false;
        StopAllCoroutines();
        Debug.Log("Boss 2 died");
    }

    // -------------------------------
    // GIZMOS
    // -------------------------------
    private void OnDrawGizmos()
    {
        if (!DrawGizmos || pathPoints == null || pathPoints.Count == 0 || BossPivotPoint == null)
            return;

        // Control points (yellow)
        Gizmos.color = Color.yellow;
        foreach (var p in pathPoints)
        {
            Gizmos.DrawSphere(GetPointOnBossOrbit(p), GizmoSphereSize);
        }

        // Start / End points
        if (pathPoints.Count >= 2)
        {
            Gizmos.color = Color.green; // Start
            Gizmos.DrawSphere(GetPointOnBossOrbit(pathPoints[0]), GizmoSphereSize * 1.4f);

            Gizmos.color = Color.red; // End
            Gizmos.DrawSphere(GetPointOnBossOrbit(pathPoints[pathPoints.Count - 1]), GizmoSphereSize * 1.4f);
        }

        // Bezier curve (yellow)
        Gizmos.color = Color.yellow;

        Vector2 prev = EvaluateBezier(pathPoints, 0f);
        Vector3 prevPos = GetPointOnBossOrbit(prev);

        for (int i = 1; i <= BezierDetail; i++)
        {
            float t = i / (float)BezierDetail;
            Vector2 next = EvaluateBezier(pathPoints, t);
            Vector3 nextPos = GetPointOnBossOrbit(next);

            Gizmos.DrawLine(prevPos, nextPos);
            prevPos = nextPos;
        }
    }

    Vector3 GetPointOnBossOrbit(Vector2 rot)
    {
        if (BossPivotPoint == null)
            return Vector3.zero;

        Vector3 localOffset = transform.localPosition.normalized;
        float radius = transform.localPosition.magnitude;

        Vector3 rotatedOffset = Quaternion.Euler(rot.x, rot.y, 0f) * localOffset;

        return BossPivotPoint.position + rotatedOffset * radius;
    }
}
