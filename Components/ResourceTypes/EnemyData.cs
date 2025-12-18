using Godot;
using System;

[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string name;
    [Export] public int value;
    [Export] public PackedScene enemyScene;
}
