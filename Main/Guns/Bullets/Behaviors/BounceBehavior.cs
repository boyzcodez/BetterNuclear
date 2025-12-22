using Godot;
using System;

public class BounceBehavior : IBulletBehavior
{
    public int bouncesTotal;
    public int bouncesLeft = 3;

    public void OnSpawn(Bullet b)
    {
        bouncesLeft = bouncesTotal;
    }

    public void OnUpdate(Bullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * b.Speed * delta);
    }

    public void OnHit(Bullet b, ICollidable collidable)
    {
    }

    public BounceBehavior(int bounces)
    {
        bouncesTotal = bounces;
        bouncesLeft = bounces;
    }

    public void OnWallHit(Bullet b, Vector2 normal)
    {
        if (bouncesLeft-- <= 0)
        {
            b.Deactivate();
            return;
        }

        b.Velocity = b.Velocity.Bounce(normal);
        b.Rotation = b.Velocity.Angle();
        b.particles.Emitting = true;
    }
}
