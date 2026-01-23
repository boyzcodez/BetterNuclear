using System;
using Godot;

[GlobalClass]
public partial class Look : Marker2D
{
    [Export] public float SnapDegrees = 5f;
    [Export] private float BaseOffset = 14f;
    [Export] private float MinOffset = 7f;

    private bool Locked = false;

    public void SetRotation(Vector2 direction)
    {
        if (Locked)
        {
            Rotation = direction.X > 0 ? Vector2.Right.Angle() : Vector2.Left.Angle();
            return;
        }

        float angle = direction.Angle();

        // Convert to degrees
        float angleDegrees = Mathf.RadToDeg(angle);

        // Snap to nearest increment (e.g., 10°)
        float snappedDegrees = Mathf.Round(angleDegrees / SnapDegrees) * SnapDegrees;

        // Convert back to radians
        float snappedAngle = Mathf.DegToRad(snappedDegrees);


        Rotation = snappedAngle;
    }

    public void Lock(bool bl)
    {
        SetRotation(Vector2.Right);
        Locked = bl;
    }
}
