using UnityEngine;

public class IKLeg : MonoBehaviour
{
    [SerializeField]
    Transform footIKTarget;
    
    [SerializeField]
    Transform poleIKTarget;

    [SerializeField]
    float poleHeight = 5f;
    
    [SerializeField]
    Transform target;
    
    [SerializeField]
    float maxDistBeforeMovingFoot = 1f;
    
    Vector3 targetLandPos = Vector3.zero;
    
    [SerializeField]
    float lerpSpeed = 150f;
    
    float maxRayDist = 4f;
    
    float rayHightOffset = 1f;
    
    [SerializeField]
    LayerMask ground;
    
    float footHight = 1;
    
    float lastDist = 1;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateFootTarget();
    }
    
    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(targetLandPos, 0.1f);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(targetLandPos, target.position) >= maxDistBeforeMovingFoot)
        {
            UpdateFootTarget();
        }
        
        Vector3 targetPos = Vector3.Slerp(footIKTarget.position, targetLandPos, lerpSpeed * Time.deltaTime);
        
        
        targetPos.y = TestFunc(targetLandPos.y, Mathf.Max(footIKTarget.position.y, targetLandPos.y) + footHight, lastDist, Vector3.Distance(RemoveY(targetLandPos), RemoveY(footIKTarget.position)));
        
        footIKTarget.position = targetPos;
        
        Vector3 tarDir = footIKTarget.position - transform.position;
        
        poleIKTarget.position = transform.position + (tarDir.normalized * (tarDir.magnitude / 2f)) + (Vector3.up * poleHeight);
    }
    
    void UpdateFootTarget()
    {
        Debug.DrawRay(target.position, -target.up * maxRayDist, Color.cyan, 1f);
        
        if (Physics.Raycast(target.position, -target.up, out RaycastHit hit, maxRayDist, ground, QueryTriggerInteraction.Ignore))
        {
            targetLandPos = hit.point + (footIKTarget.up * rayHightOffset);
            
        }
        else // we are hiting nothing
        {
            targetLandPos = target.position;
        }
        
        lastDist = Vector3.Distance(RemoveY(targetLandPos), RemoveY(footIKTarget.position));
    }
    
    float TestFunc(float yStart, float yMid, float max, float cur)
    {
        cur = cur / max; // normalized val.
        
        return Mathf.Lerp(yStart, yMid,  Mathf.Sin(Mathf.Clamp01(cur) * Mathf.PI));
    }
    
    Vector3 RemoveY(Vector3 vec)
    {
        return new Vector3(vec.x,0,vec.z);
    }
}
