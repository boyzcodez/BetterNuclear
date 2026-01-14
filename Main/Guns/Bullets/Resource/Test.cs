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
                key: b.PoolKey,
                position: b.GlobalPosition,
                velocity: dir,
                lifetime: 2f,
                damage: b.Damage,
                collisionLayer: b.Layer,
                priority: BulletPriority.Trash,
                behaviors: new BehaviorResource[] {new Normal()}
            );
        }
    }

    public override void OnWallHit(ModularBullet b, Vector2 normal)
    {
        b.Deactivate();

        //Eventbus.TriggerSpawnItem("LargeExplosion", b.GlobalPosition);
    }
}
