using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpPad : MonoBehaviour
{
    [Header("JumpPad Settings")]
    public float JumpPadForce = 10f;      // Force applied to the player
    public bool UseImpulse = true;        // If true, uses Rigidbody.AddForce with impulse

    private void Reset()
    {
        // Ensure the collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for Player tag
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
        {
            Debug.LogWarning("JumpPad: Player object has no Rigidbody!");
            return;
        }

        // Calculate upward force in local positive Y
        Vector3 jumpDirection = transform.up * JumpPadForce;

        if (UseImpulse)
            rb.AddForce(jumpDirection, ForceMode.Impulse);
        else
            rb.linearVelocity = jumpDirection; // Replace current velocity
    }
}

