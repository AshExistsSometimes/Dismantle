using UnityEngine;
using System.Collections;

public class PhysicsProp : MonoBehaviour
{
    private bool PhysicsActive = false;

    private Rigidbody rb;

    public float TriggerRadius = 2f;

    public float PhysicsOffCooldown = 5f;

    [Header("DEBUG")]
    public bool GizmosOn = false;

    private Coroutine disableRoutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        }

        PhysicsActive = false;

        // Turn off Rigidbody by default
        rb.isKinematic = true;

        // Add a sphere trigger with radius of TriggerRadius to itself
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = TriggerRadius;
        trigger.center = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !PhysicsActive)
        {
            PhysicsActive = true;
            rb.isKinematic = false;

            if (disableRoutine != null)
                StopCoroutine(disableRoutine);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && PhysicsActive)
        {
            // Start a coroutine that disables physics again after [PhysicsOffCooldown] Seconds
            disableRoutine = StartCoroutine(DisablePhysicsAfterDelay());
        }
    }

    private IEnumerator DisablePhysicsAfterDelay()
    {
        yield return new WaitForSeconds(PhysicsOffCooldown);
        PhysicsActive = false;
        rb.isKinematic = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!GizmosOn) return;

        Gizmos.color = PhysicsActive ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, (TriggerRadius / 2));
    }
#endif
}
