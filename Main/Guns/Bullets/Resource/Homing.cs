using Godot;

[GlobalClass]
public partial class Homing : BehaviorResource
{
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float Strength = 1f;
    [Export] public float AcquireRange = 700f;
    [Export] public float TurnRate = 8f;
    [Export] public float ReacquireInterval = 0.15f;

    public override IBulletBehaviorRuntime CreateRuntime()
        => new Runtime(this);

    private sealed class Runtime : IBulletBehaviorRuntime
    {
        private readonly Homing _cfg;

        private ICollidable _target;
        private float _reacquireTimer;

        public Runtime(Homing cfg) => _cfg = cfg;

        public void OnSpawn(ModularBullet b)
        {
            _reacquireTimer = 0f;
            Acquire(b);
        }

        public void OnUpdate(ModularBullet b, float delta)
        {
            // Reacquire occasionally (cheap + keeps behavior robust)
            _reacquireTimer -= delta;
            if (_reacquireTimer <= 0f || !IsTargetValid(b, _target))
            {
                _reacquireTimer = _cfg.ReacquireInterval;
                Acquire(b);
            }

            // If we have a target, steer toward it
            if (_target != null)
            {
                Vector2 to = _target._Position - b.GlobalPosition;
                if (to.LengthSquared() > 0.0001f)
                {
                    Vector2 desiredDir = to.Normalized();

                    Vector2 currentDir = b.Velocity;
                    if (currentDir.LengthSquared() < 0.0001f)
                        currentDir = desiredDir;
                    else
                        currentDir = currentDir.Normalized();

                    float currentAng = currentDir.Angle();
                    float desiredAng = desiredDir.Angle();
                    float angDiff = Mathf.Wrap(desiredAng - currentAng, -Mathf.Pi, Mathf.Pi);

                    float maxStep = _cfg.TurnRate * delta;

                    // Strength scales how much of the allowed turn we actually take this frame
                    float step = Mathf.Clamp(angDiff, -maxStep, maxStep) * _cfg.Strength;

                    Vector2 newDir = currentDir.Rotated(step);
                    b.Velocity = newDir;
                }
            }

            b.AddDisplacement(b.Velocity * b.Speed * delta);
            b.Rotation = b.Velocity.Angle();
        }

        public void OnHit(ModularBullet b, ICollidable collidable) { }
        public void OnKill(ModularBullet b, ICollidable collidable) { }
        public void OnWallHit(ModularBullet b, Vector2 normal) { }

        private void Acquire(ModularBullet b)
        {
            var main = Main.Instance;
            if (main == null)
            {
                _target = null;
                return;
            }

            _target = main.GetNearestCollidable(
                fromWorldPos: b.GlobalPosition,
                collisionLayer: b.Layer,
                maxRange: _cfg.AcquireRange
            );
        }

        private bool IsTargetValid(ModularBullet b, ICollidable t)
        {
            if (t == null) return false;

            // Layer match (same layer interacts)
            if (t.CollisionLayer != b.Layer) return false;

            // Range check
            float r = _cfg.AcquireRange;
            if (b.GlobalPosition.DistanceSquaredTo(t._Position) > r * r) return false;

            // If it’s a Hurtbox, ensure it’s still active/alive
            if (t is Hurtbox hb)
            {
                if (!hb.active) return false;
                if (hb.Health <= 0) return false;
            }

            return true;
        }
    }
}
