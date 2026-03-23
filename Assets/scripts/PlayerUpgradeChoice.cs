using System;

public class PlayerUpgradeChoice
{
    public string Title { get; }
    public string Description { get; }
    public Action Apply { get; }

    public PlayerUpgradeChoice(string title, string description, Action apply)
    {
        Title = title;
        Description = description;
        Apply = apply;
    }
}
