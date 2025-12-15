using Godot;
using System;

public partial class Hurtbox : Node2D, ICollidable
{
    public float Radius = 30f;
    public int Health = 5;

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => 2; // Enemy

    private Main main;

    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        main.hurtboxes.Add(this);
    }


    public void TakeDamage(int dmg)
    {
        GD.Print("took damage");

        Health -= dmg;
        if (Health <= 0)
        {
            GD.Print("i died");
        }
            
    }
}
