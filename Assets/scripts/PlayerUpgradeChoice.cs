using System;

public enum PlayerUpgradeCategory
{
    Basic,
    Core,
    Synergy
}

public class PlayerUpgradeChoice
{
    public string Id { get; }
    public string Title { get; }
    public string Description { get; }
    public PlayerUpgradeCategory Category { get; }
    public int Weight { get; }
    public int MaxStacks { get; }
    public string[] RequiredUpgradeIds { get; }
    public Action Apply { get; }

    public PlayerUpgradeChoice(
        string id,
        string title,
        string description,
        PlayerUpgradeCategory category,
        int weight,
        int maxStacks,
        Action apply,
        params string[] requiredUpgradeIds)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        Weight = weight;
        MaxStacks = maxStacks;
        RequiredUpgradeIds = requiredUpgradeIds ?? Array.Empty<string>();
        Apply = apply;
    }
}
