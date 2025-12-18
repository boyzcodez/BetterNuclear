using Godot;
using System;

public partial class Enemy : Node2D
{
    [Signal] public delegate void ActivationEventHandler();
    [Signal] public delegate void DeactivationEventHandler();

    public bool active = false;
    public string name;
    public EnemyPool pool {get; set;}

    public Hurtbox hurtbox;

    public override void _Ready()
    {
        Connect(SignalName.Activation, new Callable(this, nameof(Activate)));
        Connect(SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        Visible = false;
    }

    public void Activate()
    {
        active = true;
        hurtbox.active = true;
        Visible = true;
    }
    public void Deactivate()
    {
        active = false;
        hurtbox.active = false;
        Visible = false;
    }

}
