using Godot;
using System.Collections.Generic;

public readonly struct MapGenResult
{
    public readonly HashSet<Vector2I> Floors;
    public readonly Rect2I MapBounds;
    public readonly Rect2I DestructionBounds;
    public readonly Vector2I SpawnTile;

    public MapGenResult(HashSet<Vector2I> floors, Rect2I mapBounds, Rect2I destructionBounds, Vector2I spawnTile)
    {
        Floors = floors;
        MapBounds = mapBounds;
        DestructionBounds = destructionBounds;
        SpawnTile = spawnTile;
    }
}

public interface IMapAlgorithm
{
    MapGenResult Generate(MapData map);
}
