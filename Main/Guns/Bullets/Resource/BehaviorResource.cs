using Godot;
using System;

[GlobalClass]
public abstract partial class BehaviorResource : Resource, IBulletBehavior
{
    public virtual void OnInit(Bullet b)
    {
    }
    public virtual void OnSpawn(Bullet b)
    {
    }

    public virtual void OnUpdate(Bullet b, float delta)
    {
    }

    public virtual void OnHit(Bullet b, ICollidable collidable)
    {
    }
    public virtual void OnKill(Bullet b, ICollidable collidable)
    {
    }

    public virtual void OnWallHit(Bullet b, Vector2 normal)
    {
    }
}
