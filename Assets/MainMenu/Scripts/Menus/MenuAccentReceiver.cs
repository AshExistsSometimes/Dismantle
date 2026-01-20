using UnityEngine;
using UnityEngine.UI;

public class MenuAccentReceiver : MonoBehaviour
{
    [SerializeField] private Image[] images;
    private void Awake()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.RegisterAccentReceiver(this);
    }
    public void Apply(Color colour)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
                images[i].color = colour;
        }
    }
}
