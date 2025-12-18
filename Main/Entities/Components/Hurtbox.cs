using Godot;
using System;

public partial class Hurtbox : Node2D, ICollidable
{
    [Signal] public delegate void DeathEventHandler();
    public bool active = true;
    public float Radius = 15f;
    public int Health = 5;

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => 2; // Enemy

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


    public void TakeDamage(int dmg)
    {
        GD.Print("took damage");

        Health -= dmg;
        if (Health <= 0)
        {
            GD.Print("i died");
            EmitSignal("Death");
        }
            
    }
}
