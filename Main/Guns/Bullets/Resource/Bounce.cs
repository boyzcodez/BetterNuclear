using Godot;
using System;

[GlobalClass]
public partial class Bounce : BehaviorResource, IBulletBehavior
{
    [Export] public int bouncesTotal = 3;
    public int bouncesLeft = 3;

    public override void OnSpawn(ModularBullet b)
    {
        bouncesLeft = bouncesTotal;
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
        if (bouncesLeft-- <= 0)
        {
            b.Deactivate();
            return;
        }

        b.Velocity = b.Velocity.Bounce(normal);
        b.Rotation = b.Velocity.Angle();
    }
}
