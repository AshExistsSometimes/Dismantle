using UnityEngine;

public class BossWeakPoint : MonoBehaviour, IDamagable
{
    [Header("References")]
    public Boss1AI boss1;
    public Boss2AI boss2;
    public Boss3AI boss3;


    // Health
    [Header("Health")]
    public int maxHealth = 20;
    public int currentHealth = 20;
    // if I have time, glow intensity will be tied to percent of CurrentHealth/MaxHealth

    [Space]
    [Header("Debug Info")]
    public int myBoss = 0;

    #region Glow

    [Header("Glow")]
    public Renderer targetRenderer;
    public string emissionProperty = "_EmissionColor";
    public float maxEmissionIntensity = 6f;

    private Material glowMaterial;
    private Color baseEmissionColor;

    #endregion

    private bool WeakPointActive = true;

    private void Awake()
    {
        myBoss = CheckAttachedBoss();

        currentHealth = maxHealth;

        if (myBoss == 0)
        {
            Debug.LogWarning("Weak point has incorrect number of BossAI components, please check there is ONE assigned BossAI script");
        }

        SetupGlowMaterial();
    }

    public int CheckAttachedBoss()
    {
        if (boss1 != null && (boss2 == null && boss3 == null))// Checks the weak point only has Boss1AI
        {
            return 1;
        }
        else if (boss2 != null && (boss1 == null && boss3 == null))// Checks the weak point only has Boss2AI
        { 
            return 2;
        }
        else if (boss3 != null && (boss1 == null && boss2 == null))// Checks the weak point only has Boss3AI
        {
            return 3;
        }
        else// If no BossAI or multiple BossAI, nothing should happen
        {
            return 0;
        }
    }


    public void TakeDamage(int damage)
    {
        if (!WeakPointActive) { return; }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            WeakPointActive = false;
            UpdateGlow();

            gameObject.SetActive(false);// Temporary, will need to lower intensity of colour to 0 and set colour to black
        }
    }

    public void Die()
    {
        int calculatedBossDamage = 0;

        if (myBoss == 1)
        {
            calculatedBossDamage = ((boss1.MaxHealth / boss1.WeakPoints.Count) * (int)boss1.BossArmour);
            Debug.Log("Damaging Boss for " +  calculatedBossDamage + " damage");
            boss1.TakeDamage(calculatedBossDamage);
        }
        else if (myBoss == 2)
        {
            calculatedBossDamage = ((boss2.MaxHealth / boss2.WeakPoints.Count) * (int)boss2.BossArmour);
            Debug.Log("Damaging Boss for " + calculatedBossDamage + " damage");
            boss2.TakeDamage(calculatedBossDamage);
        }
        else if (myBoss == 3)
        {
            calculatedBossDamage = ((boss3.MaxHealth / boss3.WeakPoints.Count) * (int)boss3.BossArmour);
            Debug.Log("Damaging Boss for " + calculatedBossDamage + " damage");
            boss3.TakeDamage(calculatedBossDamage);
        }
        else
        {
            Debug.LogWarning("BossWeakPoint - NO ASSIGNED BOSS TO DAMAGE - Origin:" + gameObject.name);
        }
    }

    private void SetupGlowMaterial()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            Debug.LogWarning("BossWeakPoint has no Renderer assigned.");
            return;
        }

        // IMPORTANT: this creates an instance (no shared material issues)
        glowMaterial = targetRenderer.material;

        if (!glowMaterial.HasProperty(emissionProperty))
        {
            Debug.LogWarning("Material does not support emission.");
            return;
        }

        glowMaterial.EnableKeyword("_EMISSION");

        // Store base color (already HDR in most cases)
        baseEmissionColor = glowMaterial.GetColor(emissionProperty);
    }

    private void UpdateGlow()
    {
        if (glowMaterial == null) return;

        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        float intensity = maxEmissionIntensity * healthPercent;

        Color finalColor = baseEmissionColor * intensity;

        glowMaterial.SetColor(emissionProperty, finalColor);
    }
}
