using Godot;
using System;

[GlobalClass]
public partial class Test : BehaviorResource, IBulletBehavior
{
    [Export] private int bulletAmount = 10;
    public override void OnSpawn(ModularBullet b)
    {
    }

    public override void OnUpdate(ModularBullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * b.Speed * delta);
    }

    public override void OnHit(ModularBullet b, ICollidable collidable)
    {
        b.Deactivate();
    }
    public override void OnKill(ModularBullet b, ICollidable collidable)
    {
        Vector2[] dirs =
        {
            Vector2.Up,
            Vector2.Right,
            Vector2.Down,
            Vector2.Left
        };

        foreach (var dir in dirs)
        {
            BulletPool.Spawn(
                position: b.GlobalPosition,
                velocity: dir,
                bulletData: new IBulletData(
                    b.Priority,
                    b.Shape,
                    b.damageData,
                    new BehaviorResource[] {new Normal()},
                    b.Radius,
                    b.Speed,
                    b.LifeTime,
                    b.Layer,
                    b.PoolKey
                )
            );
        }
    }

    public override void OnWallHit(ModularBullet b, Vector2 normal)
    {
        b.Deactivate();

        //Eventbus.TriggerSpawnItem("LargeExplosion", b.GlobalPosition);
    }
}
