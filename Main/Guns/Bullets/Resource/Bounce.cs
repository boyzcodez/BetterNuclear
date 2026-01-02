using Godot;
using System;

[GlobalClass]
public partial class Bounce : BehaviorResource, IBulletBehavior
{
    [Export] public int bouncesTotal = 3;
    public int bouncesLeft = 3;

    public override void OnInit(Bullet b)
    {
    }
    public override void OnSpawn(Bullet b)
    {
        bouncesLeft = bouncesTotal;
    }

    public override void OnUpdate(Bullet b, float delta)
    {
        b.AddDisplacement(b.Velocity * b.Speed * delta);
    }

    public override void OnHit(Bullet b, ICollidable collidable)
    {
        b.Deactivate();
    }
    public override void OnKill(Bullet b, ICollidable collidable)
    {
    }

    public override void OnWallHit(Bullet b, Vector2 normal)
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
