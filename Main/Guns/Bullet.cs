using Godot;
using System;

public partial class Bullet : AnimatedSprite2D, ICollidable
{
    public Vector2 Velocity;
    public float Radius = 15f;
    public float LifeTime = 2f;
    public int CollisionLayer = 2; // PlayerBullet, EnemyBullet, etc

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => CollisionLayer;

    private Main main;
    public bool Active = true;
    public bool HasHit { get; private set; }
    
    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        main.bullets.Add(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Velocity * (float)delta;
        LifeTime -= (float)delta;

        if (LifeTime <= 0f) Deactivate();
    }

    public void Activate()
    {
        SetPhysicsProcess(true);
    }
    public void Deactivate()
    {
        Active = false;
        Visible = false;
        SetPhysicsProcess(false);

    }

    

    public void ResetHit()
    {
        HasHit = false;
    }

    public void MarkHit()
    {
        HasHit = true;
    }



}
