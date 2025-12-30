using Godot;
using System;

[GlobalClass]
public partial class Entity : Node2D
{
    public bool active;
    public Hurtbox hurtbox;

    
    public float KnockbackTime = 0f;
    public Vector2 KnockbackVelocity;

}
