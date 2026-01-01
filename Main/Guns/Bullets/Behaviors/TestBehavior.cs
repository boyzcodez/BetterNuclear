using Godot;
using System;
using System.Linq;

public partial class TestBehavior : IBulletBehavior
{
    private StringName key = "Flak";
    private int bulletAmount = 10;
    public void OnInit(Bullet b)
    {
        // var initData = new IBulletInitData(
        //     b.damageData,
        //     [new ApplyNormal()],
        //     b.initData.ShootAnimation,
        //     b.initData.HitAnimation,
        //     b.initData.BulletRadius,
        //     b.initData.BulletSpeed,
        //     b.initData.BulletLifeTime,
        //     b.initData.CollisionLayer,
        //     key
        // );
        // key = initData.key;

        // var amount = 100;
        // b.pool.PreparePool(initData, amount);
    }
    public void OnSpawn(Bullet b)
    {
    }

    public void OnUpdate(Bullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * b.Speed * delta);
    }

    public void OnHit(Bullet b, ICollidable collidable)
    {
        b.Deactivate();
    }
    public void OnKill(Bullet b, ICollidable collidable)
    {
        // Spawn `bulletAmount` bullets evenly around 360 degrees
        var spawnPos = b.GlobalPosition;

        for (int i = 0; i < bulletAmount; i++)
        {
            Bullet bullet = b.pool.GetBullet(key);
            if (bullet == null)
                continue;

            float angle = i * (2f * Mathf.Pi / bulletAmount);
            bullet.GlobalPosition = spawnPos;
            bullet.Rotation = angle;
            bullet.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            bullet.Activate();
        }
    }

    public void OnWallHit(Bullet b, Vector2 normal)
    {
        b.Deactivate();
    }
}
