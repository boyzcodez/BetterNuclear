using Godot;
using System;

[GlobalClass]
public partial class MapData : Resource
{
    // --------------------
    // MAP SIZE
    // --------------------
    [Export] public int ViewWidth = 50;
    [Export] public int ViewHeight = 35;
    [Export] public int MapPadding = 10;

    // --------------------
    // WALKERS
    // --------------------
    [Export] public int WalkerAmount = 6;
    [Export] public int PathLength = 100;
    [Export] public int WalkerSpawnRadius = 5;

    [Export] public float TurnChance = 0.7f;
    [Export] public float HorizontalBias = 1.0f;
    [Export] public float VerticalBias = 1.0f;

    [Export] public int StepSize = 1;
    [Export] public int CorridorWidth = 1;
    [Export] public int MaxFloorNeighbors = 2;

    // --------------------
    // ROOMS
    // --------------------
    [Export] public float RoomChance = 0.10f;
    [Export] public Vector2I RoomSizeMin = new(4, 4);
    [Export] public Vector2I RoomSizeMax = new(10, 10);

    // --------------------
    // DENSITY / LIMITS
    // --------------------
    [Export] public int MaxFloorTiles = 1200;
    [Export] public int WallPadding = 5;

    // --------------------
    // RANDOMNESS
    // --------------------
    [Export] public bool UseRandomSeed = true;
    [Export] public int Seed = 0;
}
