using Godot;

[GlobalClass]
public partial class Test : BehaviorResource
{
    [Export] private int bulletAmount = 10;

    // Reuse one definition resource (stateless config).
    // Each spawned bullet will call CreateRuntime() and get its own runtime.
    // Might need to make an export here if you want to add different behaviors
    private static readonly Normal NormalDef = new Normal();

    public override IBulletBehaviorRuntime CreateRuntime()
        => new Runtime(this);

    private sealed class Runtime : IBulletBehaviorRuntime
    {
        private readonly Test def;

        public Runtime(Test def) => this.def = def;

        public void OnSpawn(ModularBullet b) { }

        public void OnUpdate(ModularBullet b, float delta)
        {
            b.AddDisplacement(b.Velocity * b.Speed * delta);
        }

        public void OnHit(ModularBullet b, ICollidable collidable)
        {
            b.Deactivate();
        }

        public void OnKill(ModularBullet b, ICollidable collidable)
        {
            int count = Mathf.Max(1, def.bulletAmount);

            // Build child bullet data once; reuse for all spawned bullets.
            // IMPORTANT: behaviors here are *definition resources*, not runtime.
            var childData = new IBulletData(
                b.Priority,
                b.Shape,
                b.damageData,
                new BehaviorResource[] { NormalDef },
                b.Radius,
                b.Speed,
                b.LifeTime,
                b.Layer,
                b.PoolKey
            );

            float step = Mathf.Tau / count;
            float start = 0f; // you could randomize if you want

            for (int i = 0; i < count; i++)
            {
                float ang = start + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).Normalized();

                BulletPool.Spawn(
                    position: b.GlobalPosition,
                    velocity: dir,
                    bulletData: childData
                );
            }
        }

        public void OnWallHit(ModularBullet b, Vector2 normal)
        {
            b.Deactivate();
            Eventbus.TriggerSpawnItem("LargeExplosion", b.GlobalPosition);
        }
    }
}
