using Godot;
using System;

public interface IEnemyState
{
    Vector2 GetDesiredDirection(Enemy enemy);
}
