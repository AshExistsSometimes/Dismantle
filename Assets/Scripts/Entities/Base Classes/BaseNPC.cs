using UnityEngine;

public class BaseNPC : Entity
{
    [Header("<color=#264EB3><size=110%><b>NPC Settings")]
    public float WanderRadius = 3f;

    [Header("<color=#3396D4><size=110%><b>Behavior Bools")]
    public bool Wanders = false;       // Should the NPC move around?
    public bool Interactable = true;   // Can the player interact with this NPC

    // --------------------
    // Unity Functions
    // --------------------
    private void Awake()
    {
        base.Awake();

        // If interactable, set outline to white
        if (Interactable && outline != null)
        {
            Color white = Color.white;
            white.a = 1f; // ensure fully visible
            outline.OutlineColor = white;
        }
    }

    private void Update()
    {
        // For now, no behavior until AI/interaction systems are implemented
    }

    // --------------------
    // Functions
    // --------------------

    private void OnInteract()
    {
        // No interaction system yet
    }
}

