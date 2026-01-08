using Godot;
using System.Collections.Generic;

public partial class LargeExplosion : AnimatedSprite2D, ICollectable
{
    public DamageData damageData = new DamageData(0, 200f, "Explosion", "Explosion");
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
        Eventbus.TriggerExplosion(50f, GlobalPosition, damageData);
        Eventbus.TriggerScreenShake(5f, 0.3f);
    }
    public void OnDeactivation()
    {
        // return back to pool
        _Pool.ReturnItem(_Name, this);
    }
}
