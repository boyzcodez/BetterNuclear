using Godot;
using System;

public class BounceBehavior : IBulletBehavior
{
    public int bouncesLeft = 3;

    public void OnSpawn(Bullet b)
    {
    }

    public void OnUpdate(Bullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * delta);
    }

    public void OnHit(Bullet b, ICollidable collidable)
    {
    }

    public BounceBehavior(int bounces)
    {
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
    }
}
