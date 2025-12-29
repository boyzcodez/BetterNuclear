using Godot;

public partial class Explosion : AnimatedSprite2D, ICollectable
{
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
        Eventbus.TriggerExplosion(2, GlobalPosition);
        Eventbus.TriggerScreenShake(1.5f, 0.3f);
    }
    public void OnDeactivation()
    {
        // return back to pool
        _Pool.ReturnItem(_Name, this);
    }
}
