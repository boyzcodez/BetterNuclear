using Godot;
using System;

public interface ICollidable
{
    Vector2 _Position {get;}
    float CollisionRadius {get;}
    int CollisionLayer {get;}
}
