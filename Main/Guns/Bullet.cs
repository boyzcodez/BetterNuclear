using Godot;
using System;

public partial class Bullet : Area2D
{
    public bool active = false;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void Init(GunData gunData, BulletPool pool)
    {
        
    }
    public void Activate(float rotation)
    {
        
    }
    public void Deactivate()
    {
        
    }
}
