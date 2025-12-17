using Godot;
using System;

public partial class Look : Marker2D
{
    [Export] public PackedScene bul;
    [Export] public float SnapDegrees = 5f;
    [Export] private float BaseOffset = 14f;
    [Export] private float MinOffset = 7f;

    private Node2D main;
    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Node2D;
    }

    public override void _Process(double delta)
    {
        Vector2 toMouse = GetGlobalMousePosition() - GlobalPosition;

        float angle = toMouse.Angle();

        // Convert to degrees
        float angleDegrees = Mathf.RadToDeg(angle);

        // Snap to nearest increment (e.g., 10°)
        float snappedDegrees = Mathf.Round(angleDegrees / SnapDegrees) * SnapDegrees;

        // Convert back to radians
        float snappedAngle = Mathf.DegToRad(snappedDegrees);


        Rotation = snappedAngle;
    }
}
