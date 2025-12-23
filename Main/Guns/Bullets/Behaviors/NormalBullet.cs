using Godot;
using System;

public class NormalBullet : IBulletBehavior
{

    public void OnSpawn(Bullet b)
    {
    }

    public void OnUpdate(Bullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * b.Speed * delta);
    }

    public void OnHit(Bullet b, ICollidable collidable)
    {
    }

    public void OnWallHit(Bullet b, Vector2 normal)
    {
        b.Deactivate();
    }
}
