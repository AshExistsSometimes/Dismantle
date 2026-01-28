using UnityEngine;

public class BaseNPC : Entity
{
    [Header("<color=#264EB3><size=110%><b>NPC Settings")]
    public float WanderRadius = 3f;

    [Header("<color=#3396D4><size=110%><b>Behavior Bools")]
    public bool Wanders = false;

    // --------------------
    // Unity Functions
    // --------------------
    private new void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // For now, no behavior until AI/interaction systems are implemented
    }
}

