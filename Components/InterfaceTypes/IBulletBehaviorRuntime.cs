using Godot;

public interface IBulletBehaviorRuntime
{
    void OnSpawn(ModularBullet b);
    void OnUpdate(ModularBullet b, float delta);
    void OnHit(ModularBullet b, ICollidable collidable);
    void OnKill(ModularBullet b, ICollidable collidable);
    void OnWallHit(ModularBullet b, Vector2 normal);
}
