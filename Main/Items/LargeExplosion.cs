using Godot;
using System.Collections.Generic;

public partial class LargeExplosion : AnimatedSprite2D, ICollectable
{
    public string _Name;
    public Items _Pool;

    public List<Dust> dustParticles = new();

    public void Init(string name, Items pool)
    {
        _Name = name;
        _Pool = pool;

        AnimationFinished += OnDeactivation;

        foreach (Dust dust in GetChildren())
        {
            dustParticles.Add(dust);
        }
    }
    public void OnActivation()
    {
        Play("default");
        Eventbus.TriggerExplosion(2, GlobalPosition);
        Eventbus.TriggerScreenShake(2.5f, 0.3f);

        foreach(var dust in dustParticles)
        {
            dust.Play();
        }
    }
    public void OnDeactivation()
    {
        // return back to pool
        _Pool.ReturnItem(_Name, this);
    }
}
