using Godot;

public partial class LargeExplosion : AnimatedSprite2D, ICollectable
{
    [Export] private ItemResource DustExplosion;
    [Export] private ItemResource Crater;
    public DamageData damageData = new DamageData(5f, 200f, "Explosion", DamageTypes.Impact);
    public string id;
    public Items _Pool;

    public void Init(string newId, Items pool)
    {
        id = newId;
        _Pool = pool;

        AnimationFinished += OnDeactivation;
    }
    public void OnActivation()
    {
        Play("default");
        Eventbus.TriggerSpawnItem(DustExplosion.Id, GlobalPosition);
        Eventbus.TriggerSpawnItem(Crater.Id, GlobalPosition);
        Eventbus.TriggerExplosion(60f, GlobalPosition, damageData);
        Eventbus.TriggerScreenShake(10f, 0.4f);
    }
    public void OnDeactivation()
    {
        // return back to pool
        _Pool.ReturnItem(id, this);
    }
}
