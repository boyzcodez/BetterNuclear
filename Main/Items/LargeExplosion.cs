using Godot;
using System.Collections.Generic;

public partial class LargeExplosion : AnimatedSprite2D, ICollectable
{
    public DamageData damageData = new DamageData(5f, 200f, "Explosion", DamageTypes.Impact);
    public string _Name;
    public Items _Pool;

    public void Init(string name, Items pool)
    {
        _Name = name;
        _Pool = pool;

        AnimationFinished += OnDeactivation;
    }
    public void OnActivation()
    {
        Play("default");
        Eventbus.TriggerSpawnItem("DustExplosion", GlobalPosition);
        Eventbus.TriggerSpawnItem("Crater", GlobalPosition);
        Eventbus.TriggerExplosion(60f, GlobalPosition, damageData);
        Eventbus.TriggerScreenShake(10f, 0.4f);
    }
    public void OnDeactivation()
    {
        // return back to pool
        _Pool.ReturnItem(_Name, this);
    }
}
