using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerShoot))]
[RequireComponent(typeof(PlayerHealth))]
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
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        playerMovement = GetComponent<PlayerMovement>();
        playerShoot = GetComponent<PlayerShoot>();
        playerHealth = GetComponent<PlayerHealth>();

        RequiredExperience = startingRequiredExperience;
        LevelUpUI.Instance.UpdateHud(CurrentLevel, CurrentExperience, RequiredExperience);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0 || playerHealth.IsDead)
        {
            return;
        }

        CurrentExperience += amount;
        LevelUpUI.Instance.UpdateHud(CurrentLevel, CurrentExperience, RequiredExperience);

        if (!waitingForUpgradeChoice && CurrentExperience >= RequiredExperience)
        {
            OpenNextLevelUp();
        }
    }

    void OpenNextLevelUp()
    {
        if (CurrentExperience < RequiredExperience)
        {
            return;
        }

        CurrentExperience -= RequiredExperience;
        CurrentLevel += 1;
        RequiredExperience = startingRequiredExperience + (CurrentLevel - 1) * requiredExperienceGrowth;
        waitingForUpgradeChoice = true;

        List<PlayerUpgradeChoice> choices = BuildRandomChoices();
        LevelUpUI.Instance.UpdateHud(CurrentLevel, CurrentExperience, RequiredExperience);
        LevelUpUI.Instance.ShowLevelUp(CurrentLevel, choices, ApplyUpgradeChoice);
    }

    void ApplyUpgradeChoice(PlayerUpgradeChoice choice)
    {
        choice.Apply?.Invoke();
        waitingForUpgradeChoice = false;
        LevelUpUI.Instance.UpdateHud(CurrentLevel, CurrentExperience, RequiredExperience);

        if (CurrentExperience >= RequiredExperience)
        {
            OpenNextLevelUp();
        }
    }

    List<PlayerUpgradeChoice> BuildRandomChoices()
    {
        List<PlayerUpgradeChoice> pool = new List<PlayerUpgradeChoice>
        {
            new PlayerUpgradeChoice(
                "Swift Footwork",
                "Move speed +1",
                () => playerMovement.moveSpeed += 1f),

            new PlayerUpgradeChoice(
                "Heavy Rounds",
                "Bullet damage +1",
                () => playerShoot.bulletDamage += 1),

            new PlayerUpgradeChoice(
                "Rapid Trigger",
                "Shoot 12% faster",
                () => playerShoot.fireRate = Mathf.Max(0.05f, playerShoot.fireRate * 0.88f)),

            new PlayerUpgradeChoice(
                "Vitality Boost",
                "Max HP +2 and heal 2",
                () => playerHealth.IncreaseMaxHealth(2, 2)),

            new PlayerUpgradeChoice(
                "Ballistic Force",
                "Bullet speed +3",
                () => playerShoot.bulletSpeed += 3f)
        };

        List<PlayerUpgradeChoice> results = new List<PlayerUpgradeChoice>();
        while (pool.Count > 0 && results.Count < 3)
        {
            int index = Random.Range(0, pool.Count);
            results.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return results;
    }
}
