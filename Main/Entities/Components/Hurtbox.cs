using Godot;
using System;

public partial class Hurtbox : Node2D, ICollidable
{
    [Signal] public delegate void HitEventHandler(Vector2 dir, float force);
    [Signal] public delegate void DeathEventHandler();

    [Export] public int SetCollisionLayer = 2;
    [Export] public float Radius = 15f;
    [Export] public Shape2D Shape;
    [Export] public Vector2 ShapeOffset = Vector2.Zero;

    public Entity parent;

    public bool active => parent.active;
    public float Health = 5;
    [Export] public float MaxHealth = 5;
    [Export] public float DamageCap = 0;

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => SetCollisionLayer; // Enemy
    Shape2D ICollidable.CollisionShape => Shape;
    Transform2D ICollidable.CollisionXform
    {
        get
        {
            Vector2 off = ShapeOffset.Rotated(GlobalRotation);
            return new Transform2D(GlobalRotation, GlobalPosition + off);
        }
    }


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


    public void TakeDamage(DamageData damageData, Vector2 knockbackDir)
    {
        var dmg = damageData.Damage;

        if (DamageCap > 0f)
        {
            dmg = Mathf.Min(dmg, DamageCap);
        }

        Health -= dmg;
        EmitSignal("Hit", knockbackDir, damageData.Knockback);
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
