using Godot;
using System.Collections.Generic;

public partial class DustExplosion : Node2D, ICollectable
{
    public string _Name;
    public Items _Pool;

    public Timer timer;

    public List<Dust> dustParticles = new();

    public void Init(string name, Items pool)
    {
        _Name = name;
        _Pool = pool;

        foreach (var child in GetChildren())
        {
            if (child is Dust dust) dustParticles.Add(dust);
        }

        timer = GetNode<Timer>("Timer");
        timer.Timeout += OnDeactivation;
    }
    public void OnActivation()
    {
        timer.Start(0.6f);
        
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
