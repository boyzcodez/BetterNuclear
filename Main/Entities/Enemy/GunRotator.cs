using Godot;
using System;

public partial class GunRotator : Node2D
{
    [Export] private Look look;
    [Export] private Guns guns;
    private Vector2 lastFacingDirection = Vector2.Right;
    
    private Player player;
    private Enemy owner;
    

    public override void _Ready()
    {
        owner = GetParent<Enemy>();
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        owner.Connect(Enemy.SignalName.Activation, new Callable(this, nameof(Activate)));
        owner.Connect(Enemy.SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        SetPhysicsProcess(false);
        if (guns != null) guns.SetProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (look == null)
            return;

        Vector2 aimDirection;

        if (owner.InSight && player != null)
        {
            aimDirection =
                (player.GlobalPosition - look.GlobalPosition).Normalized();
        }
        else if (owner.Velocity != Vector2.Zero)
        {
            aimDirection = owner.Velocity.Normalized();
        }
        else
        {
            aimDirection = lastFacingDirection;
        }

        lastFacingDirection = aimDirection;

        look.SetRotation(aimDirection);
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
