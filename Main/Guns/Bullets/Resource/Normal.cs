using Godot;

[GlobalClass]
public partial class Normal : BehaviorResource
{
    public override IBulletBehaviorRuntime CreateRuntime()
        => new Runtime();

    private sealed class Runtime : IBulletBehaviorRuntime
    {
        public void OnSpawn(ModularBullet b) { }

        public void OnUpdate(ModularBullet b, float delta)
        {
            b.AddDisplacement(b.Velocity * b.Speed * delta);
        }

        public void OnHit(ModularBullet b, ICollidable collidable)
        {
        }

        public void OnKill(ModularBullet b, ICollidable collidable) { }

        public void OnWallHit(ModularBullet b, Vector2 normal)
        {
        }
    }
}
