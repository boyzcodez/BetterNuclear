using Godot;
using System;

public partial class Look : Marker2D
{
    [Export] public float SnapDegrees = 5f;
    [Export] private float BaseOffset = 14f;
    [Export] private float MinOffset = 7f;
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
