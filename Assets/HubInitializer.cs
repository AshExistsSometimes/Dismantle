using UnityEngine;

public class HubInitializer : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.IsPaused = false;
        GameManager.Instance.UIOpen = false;
    }
}
