using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public PlayerController player;
    public PlayerHealth health;
    public DualHooks grapple;

    [Header ("Health")]
    public TMP_Text HealthText;
    public Slider HealthSlider;
    public Image HealthFillImage;
    [Space]
    public Gradient HealthDisplayGradient;
    [Space]
    [Range (0f, 100f)] public float DebugHealthValue = 100f;

    [Header("Fuel")]
    public Slider FuelSlider;
    public Image FuelFillImage;
    [Space]
    public Gradient FuelDisplayGradient;
    public Color NotEnoughFuelColour;
    public Color OneHookOnlyColour;

    [Header("Active Equipment")]
    public Image EquipmentIcon;
    public Image SecondaryEquipmentIcon;

    [Header("References")]
    public TMP_Text InteractionText;




    private void Awake()
    {
        PlayerWeaponManager weaponManager =
            FindFirstObjectByType<PlayerWeaponManager>();

        if (weaponManager != null)
        {
            weaponManager.RegisterHUD(this);
        }
        else
        {
            Debug.LogWarning("HUDController: WeaponManager not found.");
        }

        UIManager.Instance.InteractText = InteractionText;
    }

    private void Start()
    {
        UpdateAllDisplays();
    }
    private void Update()
    {
        // UpdateHealthDisplay(100f); // No health yet
        UpdateFuelDisplay(grapple.fuel);
        UpdateHealthDisplay(health.CurrentHP);
    }

    // Currently just updates everything itself every frame, but once things are added the scripts being
    // referenced will call the below functions themselves so it isn't updating every frame

    // Health //
    public void UpdateHealthDisplay(float HealthValue)
    {
        HealthSlider.value = HealthValue;   
        HealthText.text = (HealthValue + "HP");

        HealthFillImage.color = HealthDisplayGradient.Evaluate(HealthValue / HealthSlider.maxValue);
    }

    public void UpdateHealthDisplayMax(float MaxHealthValue)
    {
        HealthSlider.maxValue = MaxHealthValue;
    }

    // Fuel //
    public void UpdateFuelDisplay(float FuelValue)
    {
        FuelSlider.value = FuelValue;

        if (FuelValue >= (grapple.singleHookCost + grapple.dualHookCost)) // Both hooks can be used
        {
            FuelFillImage.color = FuelDisplayGradient.Evaluate(FuelValue / FuelSlider.maxValue);
        }
        else if (FuelValue >= grapple.singleHookCost && FuelValue < (grapple.singleHookCost + grapple.dualHookCost))
        {
            FuelFillImage.color = OneHookOnlyColour;
        }
        else
        {
            FuelFillImage.color = NotEnoughFuelColour;
        }
        
    }

    public void UpdateFuelDisplayMax(float MaxFuelValue)
    {
        FuelSlider.maxValue = MaxFuelValue;
    }


    // Initialisation
    public void UpdateAllDisplays()
    {
        UpdateHealthDisplay(100f);
        UpdateHealthDisplayMax(100f);

        UpdateFuelDisplay(grapple.fuel);
        UpdateFuelDisplayMax(grapple.maxFuel);
    }
}
