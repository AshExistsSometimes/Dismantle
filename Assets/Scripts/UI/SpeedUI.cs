using TMPro;
using UnityEngine;

public class SpeedUI : MonoBehaviour
{
    public PlayerController player;
    public TMP_Text speedText;

    [Header("Me when I decieve and lie")]
    public bool LieToPlayer = true;
    public float SpeedLie = 1.1f;

    private float speed;

    private float shownSpeed;

    private void Update()
    {
        speed = (Mathf.Round(player.PlayerVelocity * 100)) / 100;

        if (LieToPlayer)
        {
            shownSpeed = (Mathf.Round((speed * SpeedLie) * 100)) / 100;

            speedText.text = (shownSpeed + " m/s");
        }
        else
        {
            speedText.text = (speed + " m/s");
        }
    }
}
