using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlailingEnemy : MonoBehaviour
{
    [Header("Head")]
    public Transform head;

    [Tooltip("Max yaw offset from base rotation")]
    public float headYawRange = 60f;

    [Tooltip("Min/Max wait time between head movements")]
    public Vector2 headMoveInterval = new Vector2(1f, 3f);

    [Tooltip("How fast the head rotates")]
    public float headRotateSpeed = 3f;

    [Tooltip("Chance to reset to forward (0 offset)")]
    [Range(0f, 1f)]
    public float resetChance = 0.3f;

    [Header("Legs")]
    public List<Transform> legs = new List<Transform>();

    [Tooltip("Min/Max X rotation for legs")]
    public float legMinAngle = -23f;
    public float legMaxAngle = 25f;

    [Tooltip("How fast legs move")]
    public float legSpeed = 2f;

    // Internal
    private Quaternion headBaseRotation;
    private float currentHeadTarget;

    private class LegData
    {
        public Transform leg;

        public Quaternion baseRotation;

        public float targetAngle;
        public float currentAngle;
        public float speed;
    }

    private List<LegData> legData = new List<LegData>();

    private void Start()
    {
        // Cache base head rotation
        headBaseRotation = head.localRotation;

        StartCoroutine(HeadRoutine());

        // Setup legs
        foreach (var leg in legs)
        {
            LegData data = new LegData();
            data.leg = leg;

            data.baseRotation = leg.localRotation;

            data.currentAngle = 0f;
            data.targetAngle = Random.Range(legMinAngle, legMaxAngle);
            data.speed = Random.Range(0.5f, 1.5f) * legSpeed;

            legData.Add(data);
        }
    }

    private void Update()
    {
        UpdateHead();
        UpdateLegs();
    }

    // =========================
    // HEAD
    // =========================

    private IEnumerator HeadRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(headMoveInterval.x, headMoveInterval.y));

            // Decide target
            if (Random.value < resetChance)
            {
                currentHeadTarget = 0f; // forward
            }
            else
            {
                currentHeadTarget = Random.Range(-headYawRange, headYawRange);
            }
        }
    }

    private void UpdateHead()
    {
        Quaternion targetRot =
            headBaseRotation *
            Quaternion.Euler(0f, currentHeadTarget, 0f);

        head.localRotation = Quaternion.Slerp(
            head.localRotation,
            targetRot,
            Time.deltaTime * headRotateSpeed
        );
    }

    // =========================
    // LEGS
    // =========================

    private void UpdateLegs()
    {
        foreach (var leg in legData)
        {
            // Move toward target
            leg.currentAngle = Mathf.MoveTowards(
                leg.currentAngle,
                leg.targetAngle,
                Time.deltaTime * leg.speed * 50f
            );

            // Apply rotation (LOCAL X ONLY)
            leg.leg.localRotation =
                leg.baseRotation *
                Quaternion.Euler(leg.currentAngle, 0f, 0f);

            // If reached target, pick a new one
            if (Mathf.Abs(leg.currentAngle - leg.targetAngle) < 0.5f)
            {
                leg.targetAngle = Random.Range(legMinAngle, legMaxAngle);
                leg.speed = Random.Range(0.5f, 1.5f) * legSpeed;
            }
        }
    }
}
