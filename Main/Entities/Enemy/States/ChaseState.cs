using Godot;

public class ChaseState : IEnemyState
{
    public Vector2 GetDesiredDirection(Enemy enemy)
    {
        Vector2 toPlayer = enemy.playerPos - enemy.GlobalPosition;
        return toPlayer.Normalized();
    }
}
