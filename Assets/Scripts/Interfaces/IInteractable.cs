using UnityEngine;

public interface IInteractable
{
    public void OnHoverStart();// Should get an outline - Outline sizeset to 5, colour set to white - activates when mouse hovers over the gameobject

    public void OnHoverStop();// Changable for NPCs, but by default just set outline width to 0, for NPC's, set it to their old outline width and colour - activates when mouse stops hovering over the gameobject

    public void OnInteract();// Runs when the player presses the interact key [E] while mouse is hovering over the gameobject
}
