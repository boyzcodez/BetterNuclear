using Godot;
using System;

public enum Generators
{
    Walker,
    Dungeon,
    Something
}

public partial class MapGenerator : Node2D
{
    [Export] private WalkerHead walker;
    [Export] private DungeonMaker dungeon;

    public void Generate(Generators key)
    {
        switch (key)
        {
            case Generators.Walker:
                break;
            case Generators.Dungeon:
                break;
            case Generators.Something:
                break;
            default:
                break;
        }
    }
}
