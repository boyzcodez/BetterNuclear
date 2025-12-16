using Godot;
using System;
using System.Collections.Generic;

public partial class Bullet : AnimatedSprite2D, ICollidable
{
    public Vector2 Velocity;
    public float Radius = 5f;
    public float LifeTime = 5f;
    public int CollisionLayer = 2; // PlayerBullet, EnemyBullet, etc

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => CollisionLayer;

    public List<IBulletBehavior> Behaviors = new();

    public bool Active = true;
    public bool HasHit { get; private set; }

    private Main main;

    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        main.bullets.Add(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        
        MoveAndCollideWithWalls((float)delta);

        foreach (var behavior in Behaviors) behavior.OnUpdate(this, (float)delta);

        //GlobalPosition += Velocity * (float)delta;
        LifeTime -= (float)delta;

        if (LifeTime <= 0f) Deactivate();
    }

    private void MoveAndCollideWithWalls(float delta)
    {
        Vector2 pos = GlobalPosition;
        Vector2 next = pos + Velocity * delta;

        // --- X axis ---
        if (Velocity.X != 0)
        {
            float signX = Mathf.Sign(Velocity.X);
            Vector2 probe = new Vector2(next.X + signX * Radius, pos.Y);

            if (Main.Instance.IsWallAt(probe))
            {
                Vector2 normal = new Vector2(signX, 0);
                Velocity = Velocity.Bounce(normal);
                NotifyWallHit(normal);
            }
            else
            {
                pos.X = next.X;
            }
        }

        // --- Y axis ---
        if (Velocity.Y != 0)
        {
            float signY = Mathf.Sign(Velocity.Y);
            Vector2 probe = new Vector2(pos.X, next.Y + signY * Radius);

            if (Main.Instance.IsWallAt(probe))
            {
                Vector2 normal = new Vector2(0, signY);
                Velocity = Velocity.Bounce(normal);
                NotifyWallHit(normal);
            }
            else
            {
                pos.Y = next.Y;
            }
        }

        GlobalPosition = pos;
    }

    private void NotifyWallHit(Vector2 normal)
    {
        foreach (var b in Behaviors) b.OnWallHit(this, normal);
    }

    public void Activate()
    {
        Active = true;
        Visible = true;
        SetPhysicsProcess(true);
    }
    public void Deactivate()
    {
        Active = false;
        Visible = false;
        SetPhysicsProcess(false);
    }

}
