using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthbars : MonoBehaviour
{
    public Boss1AI boss1AI;
    public Boss2AI boss2AI;
    public Boss3AI boss3AI;

    public Slider healthSlider;
    public TMP_Text BossSliderTitle;

    private void OnEnable()
    {
        InitializeBossSlider();
    }

    private void Awake()
    {
        InitializeBossSlider();
    }

    private void Update()
    {
        if (boss1AI != null)
        {
            UpdateBoss1HP();
        }

        if (boss2AI != null)
        {
            UpdateBoss2HP();
        }

        if (boss3AI != null)
        {
            UpdateBoss3HP();
        }
    }

    private void UpdateBoss1HP()
    {
        healthSlider.value = boss1AI.currentHealth;
    }

    private void UpdateBoss2HP()
    {
        healthSlider.value = boss2AI.currentHealth;
    }

    private void UpdateBoss3HP()
    {
        healthSlider.value = boss3AI.currentHealth;
    }


    private void InitialiseBoss1()
    {
        healthSlider.maxValue = boss1AI.MaxHealth;
        BossSliderTitle.text = boss1AI.BossName;
    }

    private void InitialiseBoss2()
    {
        healthSlider.maxValue = boss2AI.MaxHealth;
        BossSliderTitle.text = boss2AI.BossName;
    }

    private void InitialiseBoss3()
    {
        healthSlider.maxValue = boss3AI.MaxHealth;
        BossSliderTitle.text = boss3AI.BossName;
    }

    private void InitializeBossSlider()
    {
        if (boss1AI != null)
        {
            InitialiseBoss1();
        }

        if (boss2AI != null)
        {
            InitialiseBoss2();
        }

        if (boss3AI != null)
        {
            InitialiseBoss3();
        }
    }
}
