using Godot;
using System;

[GlobalClass]
public partial class Test : BehaviorResource, IBulletBehavior
{
    [Export] private int bulletAmount = 10;
    public override void OnInit(Bullet b)
    {
    }
    public override void OnSpawn(Bullet b)
    {
    }

    public override void OnUpdate(Bullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * b.Speed * delta);
    }

    public override void OnHit(Bullet b, ICollidable collidable)
    {
        b.Deactivate();
    }
    public override void OnKill(Bullet b, ICollidable collidable)
    {
        // Spawn `bulletAmount` bullets evenly around 360 degrees
        var spawnPos = b.GlobalPosition;

        for (int i = 0; i < bulletAmount; i++)
        {
            Bullet bullet = b.pool.GetBullet(b.key + "Copy");
            if (bullet == null)
                continue;

            float angle = i * (2f * Mathf.Pi / bulletAmount);
            bullet.GlobalPosition = spawnPos;
            bullet.Rotation = angle;
            bullet.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
            bullet.Activate();
        }
    }

    public override void OnWallHit(Bullet b, Vector2 normal)
    {
        b.Deactivate();
    }
}
