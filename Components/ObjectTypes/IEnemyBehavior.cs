using Godot;
using System;

public interface IEnemyBehavior
{
    void Enter(Enemy enemy);
    void Update(Enemy enemy, float delta);
    void Exit(Enemy enemy);
}
