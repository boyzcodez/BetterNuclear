using System;
using Godot;

[GlobalClass]
public partial class ExplosionOnSpawn : BehaviorResource
{
    [Export] private ItemResource NeededItem;

    // Reuse one definition resource (stateless config).
    // Each spawned bullet will call CreateRuntime() and get its own runtime.
    // Might need to make an export here if you want to add different behaviors
    private static readonly Normal NormalDef = new Normal();

    public override IBulletBehaviorRuntime CreateRuntime()
        => new Runtime(this, NeededItem.Id);

    private sealed class Runtime : IBulletBehaviorRuntime
    {
        private readonly ExplosionOnSpawn def;
        private readonly String itemId;

        public Runtime(ExplosionOnSpawn def, String id)
        {
            this.def = def;
            itemId = id;
        }

        public void OnSpawn(ModularBullet b)
        {
            Eventbus.TriggerSpawnItem(itemId, b.GlobalPosition);
            b.Deactivate();
        }

        public void OnUpdate(ModularBullet b, float delta)
        {
        }

        public void OnHit(ModularBullet b, ICollidable collidable)
        {
        }

        public void OnKill(ModularBullet b, ICollidable collidable)
        {
        }

        public void OnWallHit(ModularBullet b, Vector2 normal)
        {
        }
    }
}
