using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMovement), typeof(PlayerShoot), typeof(PlayerHealth))]
public class PlayerProgression : MonoBehaviour
{
    public static PlayerProgression Instance { get; private set; }

    [SerializeField] private int startingRequiredExperience = 5;
    [SerializeField] private int requiredExperienceGrowth = 3;
    [SerializeField] private KeyCode debugLevelUpKey = KeyCode.B;

    private PlayerMovement playerMovement;
    private PlayerShoot playerShoot;
    private PlayerHealth playerHealth;
    private bool waitingForUpgradeChoice;
    private readonly Dictionary<string, int> upgradeStacks = new Dictionary<string, int>();
    private float heavyRoundsProgress;
    private float vitalityProgress;

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

    void Update()
    {
        if (Input.GetKeyDown(debugLevelUpKey))
        {
            TriggerDebugLevelUp();
        }
    }

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
        List<PlayerUpgradeChoice> choices = BuildRandomChoices();
        if (choices.Count == 0)
        {
            waitingForUpgradeChoice = false;
            UpdateUI();
            return;
        }

        waitingForUpgradeChoice = true;

        if (LevelUpUI.Instance != null)
        {
            UpdateUI();
            LevelUpUI.Instance.ShowLevelUp(CurrentLevel, choices, ApplyUpgradeChoice);
        }
    }

    void ApplyUpgradeChoice(PlayerUpgradeChoice choice)
    {
        choice.Apply?.Invoke();
        upgradeStacks[choice.Id] = GetUpgradeStack(choice.Id) + 1;
        waitingForUpgradeChoice = false;
        UpdateUI();
        if (CurrentExperience >= RequiredExperience) OpenNextLevelUp();
    }

    List<PlayerUpgradeChoice> BuildRandomChoices()
    {
        List<PlayerUpgradeChoice> availableChoices = BuildAvailableChoices();
        List<PlayerUpgradeChoice> selectedChoices = new List<PlayerUpgradeChoice>();

        TryAddCategoryChoice(availableChoices, selectedChoices, PlayerUpgradeCategory.Basic);

        while (availableChoices.Count > 0 && selectedChoices.Count < 3)
        {
            PlayerUpgradeChoice choice = TakeWeightedChoice(availableChoices);
            if (choice == null)
            {
                break;
            }

            selectedChoices.Add(choice);
            RemoveChoice(availableChoices, choice.Id);
        }

        return selectedChoices;
    }

    List<PlayerUpgradeChoice> BuildAvailableChoices()
    {
        List<PlayerUpgradeChoice> allChoices = new List<PlayerUpgradeChoice>
        {
            new PlayerUpgradeChoice("swift_footwork", "Swift Footwork", $"Speed +{FormatFloat(GetBasicGain("swift_footwork", 1f))}", PlayerUpgradeCategory.Basic, 10, 0,
                ApplySwiftFootworkUpgrade),
            new PlayerUpgradeChoice("heavy_rounds", "Heavy Rounds", $"Damage growth +{FormatFloat(GetBasicGain("heavy_rounds", 1f))}", PlayerUpgradeCategory.Basic, 10, 0,
                ApplyHeavyRoundsUpgrade),
            new PlayerUpgradeChoice("rapid_trigger", "Rapid Trigger", $"Fire rate {FormatPercent(GetRapidTriggerPercent())} up", PlayerUpgradeCategory.Basic, 10, 0,
                ApplyRapidTriggerUpgrade),
            new PlayerUpgradeChoice("vitality", "Vitality", $"Max HP growth +{FormatFloat(GetBasicGain("vitality", 2f))}", PlayerUpgradeCategory.Basic, 8, 0,
                ApplyVitalityUpgrade),
            new PlayerUpgradeChoice("force", "Force", $"Bullet speed +{FormatFloat(GetBasicGain("force", 3f))}", PlayerUpgradeCategory.Basic, 8, 0,
                ApplyForceUpgrade),

            new PlayerUpgradeChoice("split_shot_core", "Split Shot", "Shots split into two side bolts", PlayerUpgradeCategory.Core, 2, 1,
                () => playerShoot.splitShotSideCount = Mathf.Max(playerShoot.splitShotSideCount, 1)),

            new PlayerUpgradeChoice("split_barrage", "Split Barrage", "One more side bolt per side", PlayerUpgradeCategory.Synergy, 3, 2,
                () => playerShoot.splitShotSideCount += 1, "split_shot_core"),
            new PlayerUpgradeChoice("split_charge", "Split Charge", "Split bolt damage +1", PlayerUpgradeCategory.Synergy, 3, 3,
                () => playerShoot.splitShotDamageBonus += 1, "split_shot_core"),
            new PlayerUpgradeChoice("split_velocity", "Split Velocity", "Split bolt speed +2", PlayerUpgradeCategory.Synergy, 2, 2,
                () => playerShoot.splitShotSpeedBonus += 2f, "split_shot_core"),

            new PlayerUpgradeChoice("frost_shot_core", "Frost Shot", "Hits chill enemies. 3 chill stacks freeze them", PlayerUpgradeCategory.Core, 3, 1,
                () => playerShoot.frostShotEnabled = true)
        };

        List<PlayerUpgradeChoice> filteredChoices = new List<PlayerUpgradeChoice>();
        foreach (PlayerUpgradeChoice choice in allChoices)
        {
            if (IsChoiceAvailable(choice))
            {
                filteredChoices.Add(choice);
            }
        }

        return filteredChoices;
    }

    bool IsChoiceAvailable(PlayerUpgradeChoice choice)
    {
        if (choice.MaxStacks > 0 && GetUpgradeStack(choice.Id) >= choice.MaxStacks)
        {
            return false;
        }

        foreach (string requiredUpgradeId in choice.RequiredUpgradeIds)
        {
            if (GetUpgradeStack(requiredUpgradeId) <= 0)
            {
                return false;
            }
        }

        return true;
    }

    int GetUpgradeStack(string upgradeId)
    {
        int currentStack;
        return upgradeStacks.TryGetValue(upgradeId, out currentStack) ? currentStack : 0;
    }

    void TryAddCategoryChoice(List<PlayerUpgradeChoice> availableChoices, List<PlayerUpgradeChoice> selectedChoices, PlayerUpgradeCategory category)
    {
        List<PlayerUpgradeChoice> categoryChoices = availableChoices.FindAll(choice => choice.Category == category);
        if (categoryChoices.Count == 0)
        {
            return;
        }

        PlayerUpgradeChoice selectedChoice = TakeWeightedChoice(categoryChoices);
        if (selectedChoice == null)
        {
            return;
        }

        selectedChoices.Add(selectedChoice);
        RemoveChoice(availableChoices, selectedChoice.Id);
    }

    PlayerUpgradeChoice TakeWeightedChoice(List<PlayerUpgradeChoice> choices)
    {
        if (choices == null || choices.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        foreach (PlayerUpgradeChoice choice in choices)
        {
            totalWeight += Mathf.Max(1, choice.Weight);
        }

        int roll = Random.Range(0, totalWeight);
        foreach (PlayerUpgradeChoice choice in choices)
        {
            roll -= Mathf.Max(1, choice.Weight);
            if (roll < 0)
            {
                return choice;
            }
        }

        return choices[choices.Count - 1];
    }

    void RemoveChoice(List<PlayerUpgradeChoice> choices, string id)
    {
        choices.RemoveAll(choice => choice.Id == id);
    }

    float GetBasicGain(string upgradeId, float baseGain)
    {
        int stack = GetUpgradeStack(upgradeId);
        float factor;

        if (stack < 3)
        {
            factor = 1f;
        }
        else if (stack < 6)
        {
            factor = 0.7f;
        }
        else if (stack < 10)
        {
            factor = 0.45f;
        }
        else
        {
            factor = 0.3f;
        }

        return baseGain * factor;
    }

    float GetRapidTriggerPercent()
    {
        return GetBasicGain("rapid_trigger", 12f);
    }

    string FormatFloat(float value)
    {
        if (Mathf.Approximately(value, Mathf.Round(value)))
        {
            return Mathf.RoundToInt(value).ToString();
        }

        return value.ToString("0.##");
    }

    string FormatPercent(float value)
    {
        return Mathf.Max(0.1f, value).ToString("0.#") + "%";
    }

    void ApplySwiftFootworkUpgrade()
    {
        playerMovement.moveSpeed += GetBasicGain("swift_footwork", 1f);
    }

    void ApplyHeavyRoundsUpgrade()
    {
        heavyRoundsProgress += GetBasicGain("heavy_rounds", 1f);
        int gainedDamage = Mathf.FloorToInt(heavyRoundsProgress + 0.0001f);
        if (gainedDamage > 0)
        {
            playerShoot.bulletDamage += gainedDamage;
            heavyRoundsProgress -= gainedDamage;
        }
    }

    void ApplyRapidTriggerUpgrade()
    {
        float reduction = Mathf.Clamp01(GetRapidTriggerPercent() / 100f);
        playerShoot.fireRate *= 1f - reduction;
    }

    void ApplyVitalityUpgrade()
    {
        vitalityProgress += GetBasicGain("vitality", 2f);
        int gainedHealth = Mathf.FloorToInt(vitalityProgress + 0.0001f);
        if (gainedHealth > 0)
        {
            playerHealth.IncreaseMaxHealth(gainedHealth, gainedHealth);
            vitalityProgress -= gainedHealth;
        }
    }

    void ApplyForceUpgrade()
    {
        playerShoot.bulletSpeed += GetBasicGain("force", 3f);
    }

    void TriggerDebugLevelUp()
    {
        if (playerHealth != null && playerHealth.CurrentHealth <= 0)
        {
            return;
        }

        int neededExperience = Mathf.Max(1, RequiredExperience - CurrentExperience);
        AddExperience(neededExperience);
    }
}

