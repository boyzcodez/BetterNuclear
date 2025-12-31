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

        if (normal.LengthSquared() <= 1e-12f) return;
        b.Velocity = b.Velocity.Bounce(normal); // already unit from WallGrid
        //b.particles.Emitting = true;

        //Eventbus.TriggerSpawnItem("LargeExplosion", b.GlobalPosition);
    }
}
