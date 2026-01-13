using Godot;
using System;

[GlobalClass]
public partial class Normal : BehaviorResource, IBulletBehavior
{
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
    }

    public override void OnWallHit(ModularBullet b, Vector2 normal)
    {
        b.Deactivate();
    }
}
