using Godot;

public interface ICollidable
{
    Vector2 _Position {get;}
    float CollisionRadius {get;}
    int CollisionLayer {get;}
}
