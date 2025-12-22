using Godot;
using System;

public partial class EnemyLook : Marker2D
{
    private Enemy owner;
    private Player player;
    private RayCast2D Raycast;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        Raycast = GetNode<RayCast2D>("Raycast");
        owner = GetParent<Enemy>();

        owner.Connect(Enemy.SignalName.Activation, new Callable(this, nameof(Activate)));
        owner.Connect(Enemy.SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        Raycast.Enabled = false;
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player != null)
        {
            LookAt(player.GlobalPosition);
        }

        if (Raycast.IsColliding() && Raycast.GetCollider() == player)
        {
            owner.InSight = true;
        }
        else
        {
            owner.InSight = false;
        } 
    }

    public void Activate()
    {
        Raycast.Enabled = true;
        SetPhysicsProcess(true);
    }
    public void Deactivate()
    {
        Raycast.Enabled = false;
        SetPhysicsProcess(false);
    }
}
