using Godot;
using System;

public class WanderState : IEnemyState
{
    private Vector2 wanderDir;

    public WanderState()
    {
        wanderDir = Vector2.FromAngle((float)GD.RandRange(0, Mathf.Tau));
    }

    public Vector2 GetDesiredDirection(Enemy enemy)
    {
        return wanderDir;
    }
}
