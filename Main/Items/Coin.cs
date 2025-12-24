using Godot;
using System;

public partial class Coin : AnimatedSprite2D, ICollectable
{
    private Player player;
    private Trail trail;
    private float speed = 250f;
    private float pickupThreshold = 10f;

    public string _Name;
    public Items _Pool;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        trail = GetNode<Trail>("Trail");
        SetPhysicsProcess(false);
    }

    public void Init(string name, Items pool)
    {
        _Name = name;
        _Pool = pool;
    }
    public void OnActivation()
    {
        ToggleRotation(true);
        Play("default");
        SetPhysicsProcess(true);
    }
    public void OnDeactivation()
    {
        ToggleRotation(false);
        Stop();
        SetPhysicsProcess(false);

        // return back to pool
        _Pool.ReturnItem(_Name, this);
    }

    public override void _PhysicsProcess(double delta)
    {
        var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
        Vector2 dir = (player.GlobalPosition - GlobalPosition).Normalized();
        
        GlobalPosition += dir * speed * (float)delta;
        trail.Update(GlobalPosition);

        if (distance < pickupThreshold)
        {
            OnDeactivation();
            GD.Print("Collected Ammo");
        }
    }

    private Tween rotateTween;

    public void ToggleRotation(bool enabled)
    {
        rotateTween?.Kill();

        if (enabled)
        {
            rotateTween = CreateTween();
            rotateTween.SetLoops();
            rotateTween.TweenProperty(this, "rotation", Mathf.Tau, 0.5f);
        }
    }
        
}
