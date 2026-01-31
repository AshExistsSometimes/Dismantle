using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    [Header("Look Target")]
    public float LookRadius = 6f;

    [Header("Rotation Limits (Degrees)")]
    public Vector2 YawLimits = new(-60f, 60f);     // Left / Right
    public Vector2 PitchLimits = new(-20f, 20f);   // Up / Down

    [Header("Smoothing")]
    public float maxLookSpeed = 6f;

    private Transform playerCamera;
    private Quaternion defaultLocalRotation;

    private void Awake()
    {
        defaultLocalRotation = transform.localRotation;
    }

    private void Start()
    {
        if (Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (playerCamera == null)
            return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);

        if (distance <= LookRadius && CanLookAtPlayer())
        {
            LookAtPlayer();
        }
        else
        {
            ReturnToDefault();
        }
    }

    private bool CanLookAtPlayer()
    {
        Vector3 direction = playerCamera.position - transform.position;
        Quaternion targetWorldRotation = Quaternion.LookRotation(direction, Vector3.up);

        Quaternion targetLocalRotation =
            Quaternion.Inverse(transform.parent.rotation) * targetWorldRotation;

        Vector3 euler = targetLocalRotation.eulerAngles;

        float pitch = NormalizeAngle(euler.x);
        float yaw = NormalizeAngle(euler.y);

        return pitch >= PitchLimits.x && pitch <= PitchLimits.y
            && yaw >= YawLimits.x && yaw <= YawLimits.y;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = playerCamera.position - transform.position;
        Quaternion targetWorldRotation = Quaternion.LookRotation(direction, Vector3.up);

        Quaternion targetLocalRotation =
            Quaternion.Inverse(transform.parent.rotation) * targetWorldRotation;

        Vector3 euler = targetLocalRotation.eulerAngles;

        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = defaultLocalRotation.eulerAngles.z;

        Quaternion finalRotation = Quaternion.Euler(euler);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            finalRotation,
            maxLookSpeed * Time.deltaTime
        );
    }

    private void ReturnToDefault()
    {
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            defaultLocalRotation,
            maxLookSpeed * Time.deltaTime
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}