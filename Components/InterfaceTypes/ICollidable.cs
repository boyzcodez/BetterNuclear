using Godot;

public interface ICollidable
{
    Vector2 _Position {get;}
    float CollisionRadius {get;}
    int CollisionLayer {get;}

    Shape2D CollisionShape {get;}
    Transform2D CollisionXform {get;}
}
