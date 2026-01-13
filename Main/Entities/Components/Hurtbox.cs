using Godot;
using System;

public partial class Hurtbox : Node2D, ICollidable
{
    [Signal] public delegate void HitEventHandler(Vector2 dir, float force);
    [Signal] public delegate void DeathEventHandler();

    [Export] public int SetCollisionLayer = 2;
    [Export] public float Radius = 15f;

    public Entity parent;

    public bool active => parent.active;
    public float Health = 5;
    [Export] public float MaxHealth = 5;

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => SetCollisionLayer; // Enemy

    private Main main;

    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        main.hurtboxes.Add(this);

        Health = MaxHealth;

        if (GetParent() is Entity _parent)
        {
            _parent.hurtbox = this;
            parent = _parent;
        } 
    }


    public void TakeDamage(float damage, float Knockback, Vector2 knockbackDir)
    {
        Health -= damage;
        EmitSignal("Hit", knockbackDir, Knockback);
        GD.Print("took damage");

        if (Health <= 0)
        {
            EmitSignal("Death");
        }
            
    }

    public void ResetHealth()
    {
        Health = MaxHealth;
    }
}
