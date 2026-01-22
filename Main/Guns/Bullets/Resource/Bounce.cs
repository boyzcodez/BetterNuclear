using Godot;

[GlobalClass]
public partial class Bounce : BehaviorResource
{
    [Export] public int BouncesTotal = 3;

    public override IBulletBehaviorRuntime CreateRuntime()
        => new BounceRuntime(this);

    private sealed class BounceRuntime : IBulletBehaviorRuntime
    {
        private readonly Bounce _def;
        private int _bouncesLeft;

        public BounceRuntime(Bounce def) => _def = def;

        public void OnSpawn(ModularBullet b) => _bouncesLeft = _def.BouncesTotal;

        public void OnUpdate(ModularBullet b, float delta)
        {
            b.AddDisplacement(b.Velocity * b.Speed * delta);
        }

        public void OnHit(ModularBullet b, ICollidable collidable) => b.Deactivate();
        public void OnKill(ModularBullet b, ICollidable collidable) { }

        public void OnWallHit(ModularBullet b, Vector2 normal)
        {
            if (_bouncesLeft-- <= 0)
            {
                b.Deactivate();
                return;
            }

            b.Velocity = b.Velocity.Bounce(normal);
            b.Rotation = b.Velocity.Angle();
        }
    }
}