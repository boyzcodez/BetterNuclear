using Godot;
using System.Collections.Generic;

public partial class WalkerHead : Node2D
{
    [Export] public int MapLength = 100;
    [Export] public int PathLength = 100;

    [Export] public TileMapLayer FloorMap;
    [Export] public TileMapLayer WallMap;

    public List<Vector2I> floorSet;
    private Player player;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        GenerateMap();
    }

    public void GenerateMap()
    {
        FloorMap.Clear();
        WallMap.Clear();

        foreach (WalkerUnit walker in GetChildren())
        {
            walker.CalcPaht();
        }

        BuildWalls();
    }

    public void BuildWalls()
    {
        floorSet = new();

        foreach (WalkerUnit walker in GetChildren())
        {
            foreach (var pos in walker.carvedTiles)
            {
                if (!floorSet.Contains(pos)) floorSet.Add(pos);
            }
        }
            

        for (int x = -MapLength; x < MapLength; x++)
        {
            for (int y = -MapLength; y < MapLength; y++)
            {
                var location = new Vector2I(x, y);
                if (!floorSet.Contains(location))
                {
                    WallMap.SetCell(location, 0, new Vector2I(1, 0));
                    FloorMap.SetCell(location, 0, new Vector2I(2,1));
                }
                 
            }
        }

        player.GlobalPosition = floorSet[floorSet.Count - 1] * 32 + new Vector2(16,16);
        
    }

    // public override void _Input(InputEvent input)
    // {
    //     if (input.IsActionPressed("space"))
    //     {
    //         Spawner.CalcRound();
    //     }
    // }

    public void Explosion(int size, Vector2 position)
    {
        Vector2I centerPos = FloorMap.LocalToMap(position);

        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                Vector2I wallPos = centerPos + new Vector2I(x, y);
                if (Mathf.Sqrt(x * x + y * y) <= size && WallMap.GetCellSourceId(wallPos) != -1)
                {
                    DestroyWall(wallPos);
                }
            }
        }
    }

    public void DestroyWall(Vector2I pos)
    {
        WallMap.EraseCell(pos);
    }
}
