using Godot;
using System;

public interface IBulletBehavior
{
    void OnSpawn(Bullet b);
    void OnUpdate(Bullet b, float delta);
    void OnHit(Bullet b, ICollidable target);
    void OnWallHit(Bullet b, Vector2 normal);
}
