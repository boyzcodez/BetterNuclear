using Godot;
using System;

public interface IBulletBehavior
{
    void OnSpawn(ModularBullet b);
    void OnUpdate(ModularBullet b, float delta);
    void OnHit(ModularBullet b, ICollidable target);
    void OnKill(ModularBullet b, ICollidable target);
    void OnWallHit(ModularBullet b, Vector2 normal);
}
