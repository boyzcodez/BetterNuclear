using Godot;
using System;

public partial class Hurtbox : Node2D, ICollidable
{
    [Signal] public delegate void HitEventHandler(Vector2 dir, float force);
    [Signal] public delegate void DeathEventHandler();

    [Export] public int SetCollisionLayer = 2;
    [Export] public float Radius = 15f;

    public bool active = true;
    public int Health = 5;

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => SetCollisionLayer; // Enemy

    private Main main;

    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        main.hurtboxes.Add(this);

        if (GetParent() is Enemy)
        {
            var parent = GetParent() as Enemy;
            Death += parent.Deactivate;
            parent.hurtbox = this;
            active = false;
        }
        
    }


    public void TakeDamage(DamageData damageData, Vector2 knockbackDir)
    {
        Health -= damageData.Damage;

        EmitSignal("Hit", knockbackDir, damageData.Knockback);

        if (Health <= 0)
        {
            EmitSignal("Death");
        }
            
    }
}
