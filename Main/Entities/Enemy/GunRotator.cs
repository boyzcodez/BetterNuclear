using Godot;
using System;

public partial class GunRotator : Marker2D
{
    [Export] private Marker2D look;
    [Export] private float SnapDegrees = 5f;
    [Export] private float BaseOffset = 14f;
    [Export] private float MinOffset = 7f;

    public float angle;
    private float lastFacingAngle = 0f;
    
    private Player player;
    private Enemy owner;
    private Guns guns;

    public override void _Ready()
    {
        owner = GetParent<Enemy>();
        guns = GetNodeOrNull<Guns>("Guns");
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        owner.Connect(Enemy.SignalName.Activation, new Callable(this, nameof(Activate)));
        owner.Connect(Enemy.SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        SetPhysicsProcess(false);
        if (guns != null) guns.SetProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 vel = owner.Velocity;

        if (owner.InSight)
        {
            angle = look.Rotation;
            lastFacingAngle = angle;
        }
        else if (vel != Vector2.Zero)
        {
            angle = vel.Angle();
            lastFacingAngle = angle;
        }
        else
        {
            angle = lastFacingAngle;
        }

        float angleDegrees = Mathf.RadToDeg(angle);

        // Snap to nearest increment (e.g., 10°)
        float snappedDegrees = Mathf.Round(angleDegrees / SnapDegrees) * SnapDegrees;

        // Convert back to radians
        float snappedAngle = Mathf.DegToRad(snappedDegrees);


        Rotation = snappedAngle;
    }

    public void Activate()
    {
        if (guns != null)
        {
            guns.SetProcess(true);
            guns.Visible = true;
        } 
        SetPhysicsProcess(true);
        
    }
    public void Deactivate()
    {
        if (guns != null)
        {
            guns.SetProcess(false);
            guns.Visible = false;
        } 
        SetPhysicsProcess(false);
    }
}
