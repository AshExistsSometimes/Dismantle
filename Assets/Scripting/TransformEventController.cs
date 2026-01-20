using UnityEngine;

public class TransformEventController : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 openLocalRotation = new Vector3(78f, 0f, 0f);
    public float rotateSpeed = 6f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(openLocalRotation);
        targetRotation = closedRotation;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    public void Open()
    {
        targetRotation = openRotation;
    }

    public void Close()
    {
        targetRotation = closedRotation;
    }
}
