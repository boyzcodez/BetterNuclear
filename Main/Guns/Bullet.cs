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
        
        //MoveAndCollideWithWalls((float)delta);
        MoveWithGridRay((float)delta);

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
                //Velocity = Velocity.Bounce(normal);
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
                //Velocity = Velocity.Bounce(normal);
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


    private void MoveWithGridRay(float delta)
{
    float maxDist = Velocity.Length() * delta;
    if (maxDist <= 0f)
        return;

    Vector2 dir = Velocity.Normalized();
    Vector2 pos = GlobalPosition;

    float tileSize = 32f; //fix this
    const int MAX_STEPS = 32; // safety cap

    float remaining = maxDist;

    for (int step = 0; step < MAX_STEPS && remaining > 0f; step++)
    {
        Vector2I cell = Main.Instance.WorldToCell(pos);

        int stepX = dir.X > 0 ? 1 : -1;
        int stepY = dir.Y > 0 ? 1 : -1;

        float nextX = (cell.X + (stepX > 0 ? 1 : 0)) * tileSize;
        float nextY = (cell.Y + (stepY > 0 ? 1 : 0)) * tileSize;

        float distX = dir.X != 0
            ? (nextX - pos.X) / dir.X
            : float.PositiveInfinity;

        float distY = dir.Y != 0
            ? (nextY - pos.Y) / dir.Y
            : float.PositiveInfinity;

        // Force positive distances
        if (distX < 0f) distX = float.PositiveInfinity;
        if (distY < 0f) distY = float.PositiveInfinity;

        float travel = Mathf.Min(distX, distY);

        // Clamp travel so we ALWAYS move
        if (travel <= 0f)
            travel = 0.0001f;

        if (travel > remaining)
        {
            pos += dir * remaining;
            break;
        }

        pos += dir * travel;
        remaining -= travel;

        bool hitX = distX < distY;
        Vector2I nextCell = cell + new Vector2I(
            hitX ? stepX : 0,
            hitX ? 0 : stepY
        );

        if (Main.Instance.IsWallCell(nextCell))
        {
            Vector2 normal = hitX
                ? new Vector2(-stepX, 0)
                : new Vector2(0, -stepY);

            GlobalPosition = pos;
            NotifyWallHit(normal);
            return;
        }

        // Nudge into next cell deterministically
        pos += hitX
            ? new Vector2(stepX * 0.001f, 0)
            : new Vector2(0, stepY * 0.001f);
    }

    GlobalPosition = pos;
}


}
