using Godot;
using System;

[GlobalClass]
public abstract partial class BehaviorResource : Resource, IBulletBehavior
{
    public virtual void OnSpawn(ModularBullet b)
    {
    }

    public virtual void OnUpdate(ModularBullet b, float delta)
    {
    }

    public virtual void OnHit(ModularBullet b, ICollidable collidable)
    {
    }
    public virtual void OnKill(ModularBullet b, ICollidable collidable)
    {
    }

    public virtual void OnWallHit(ModularBullet b, Vector2 normal)
    {
    }
}
