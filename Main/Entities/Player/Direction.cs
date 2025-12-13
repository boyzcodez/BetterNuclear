using Godot;
using System;

public partial class Direction : Node
{
    private string currentDirection = "Front";
    [Export] private Marker2D lookAt;
    [Export] private AnimatedSprite2D animatedSprite;

    private readonly (bool showBehind, bool flipH, string dir)[] sectionMap =
    {
        (false, false, "FrontRight"), // 0
        (false, false, "FrontRight"), // 1
        (false, true,  "Front"),      // 2
        (false, true,  "FrontRight"), // 3
        (false, true,  "FrontRight"), // 4
        (true,  true,  "BackRight"),  // 5
        (true,  false, "Back"),       // 6
        (true,  false, "BackRight"),  // 7
    };

    // public override void _Ready()
    // {
    //     lookAt = GetNode<Marker2D>("../../../LookAt");
    //     animatedSprite = GetNode<AnimatedSprite2D>("../..");
    // }

    public string GetDirection(int section)
    {
        if (section >= 0 && section < sectionMap.Length)
        {
            var settings = sectionMap[section];
            lookAt.ShowBehindParent = settings.showBehind;
            animatedSprite.FlipH = settings.flipH;
            return settings.dir;
        }

        return currentDirection;
    }
}
