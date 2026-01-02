using Godot;
using System;

[GlobalClass]
public partial class Normal : BehaviorResource, IBulletBehavior
{
    public override void OnInit(Bullet b)
    {
        GD.Print(b.damageData.Damage);
    }
    public override void OnSpawn(Bullet b)
    {
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
        b.Deactivate();
    }
}
