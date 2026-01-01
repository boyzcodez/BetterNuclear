using Godot;
using System;

public interface IBulletBehavior
{
    void OnInit(Bullet b);
    void OnSpawn(Bullet b);
    void OnUpdate(Bullet b, float delta);
    void OnHit(Bullet b, ICollidable target);
    void OnKill(Bullet b, ICollidable target);
    void OnWallHit(Bullet b, Vector2 normal);
}
