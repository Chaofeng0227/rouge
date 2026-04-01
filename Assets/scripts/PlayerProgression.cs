using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMovement), typeof(PlayerShoot), typeof(PlayerHealth))]
public class PlayerProgression : MonoBehaviour
{
    public static PlayerProgression Instance { get; private set; }

    [SerializeField] private int startingRequiredExperience = 5;
    [SerializeField] private int requiredExperienceGrowth = 3;

    private PlayerMovement playerMovement;
    private PlayerShoot playerShoot;
    private PlayerHealth playerHealth;
    private bool waitingForUpgradeChoice;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int RequiredExperience { get; private set; }

    void Awake()
    {
        Instance = this;
        playerMovement = GetComponent<PlayerMovement>();
        playerShoot = GetComponent<PlayerShoot>();
        playerHealth = GetComponent<PlayerHealth>();
        RequiredExperience = startingRequiredExperience;
    }

    void Start() { UpdateUI(); }

    public void AddExperience(int amount)
    {
        // 关键修复：根据报错，尝试访问 playerHealth.CurrentHealth
        // 如果你的 PlayerHealth 里的变量是小写 health，请改为 playerHealth.health
        if (amount <= 0 || (playerHealth != null && playerHealth.CurrentHealth <= 0)) return;

        CurrentExperience += amount;
        UpdateUI();

        if (!waitingForUpgradeChoice && CurrentExperience >= RequiredExperience) OpenNextLevelUp();
    }

    void UpdateUI() { if (LevelUpUI.Instance != null) LevelUpUI.Instance.UpdateHud(CurrentLevel, CurrentExperience, RequiredExperience); }

    void OpenNextLevelUp()
    {
        if (CurrentExperience < RequiredExperience) return;
        CurrentExperience -= RequiredExperience;
        CurrentLevel += 1;
        RequiredExperience = startingRequiredExperience + (CurrentLevel - 1) * requiredExperienceGrowth;
        waitingForUpgradeChoice = true;

        if (LevelUpUI.Instance != null)
        {
            UpdateUI();
            LevelUpUI.Instance.ShowLevelUp(CurrentLevel, BuildRandomChoices(), ApplyUpgradeChoice);
        }
    }

    void ApplyUpgradeChoice(PlayerUpgradeChoice choice)
    {
        choice.Apply?.Invoke();
        waitingForUpgradeChoice = false;
        UpdateUI();
        if (CurrentExperience >= RequiredExperience) OpenNextLevelUp();
    }

    List<PlayerUpgradeChoice> BuildRandomChoices()
    {
        List<PlayerUpgradeChoice> pool = new List<PlayerUpgradeChoice> {
            new PlayerUpgradeChoice("Swift Footwork", "Speed +1", () => playerMovement.moveSpeed += 1f),
            new PlayerUpgradeChoice("Heavy Rounds", "Damage +1", () => playerShoot.bulletDamage += 1),
            new PlayerUpgradeChoice("Rapid Trigger", "Speed 12% Up", () => playerShoot.fireRate *= 0.88f),
            new PlayerUpgradeChoice("Vitality", "Max HP +2", () => playerHealth.IncreaseMaxHealth(2, 2)),
            new PlayerUpgradeChoice("Force", "Bullet Speed +3", () => playerShoot.bulletSpeed += 3f)
        };
        List<PlayerUpgradeChoice> res = new List<PlayerUpgradeChoice>();
        while (pool.Count > 0 && res.Count < 3) { int idx = Random.Range(0, pool.Count); res.Add(pool[idx]); pool.RemoveAt(idx); }
        return res;
    }
}