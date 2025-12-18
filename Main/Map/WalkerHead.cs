using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class WalkerHead : Node2D
{
    [Export] public int MapLength = 100;
    [Export] public int PathLength = 100;

    [Export] public TileMapLayer UnderGround;
    [Export] public TileMapLayer GroundMap;
    [Export] public TileMapLayer WallMap;

    public Godot.Collections.Array<Godot.Vector2I> floorSet = [];
    private Player player;
    private Main main;

    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        GenerateMap();
    }

    public void GenerateMap()
    {
        GroundMap.Clear();
        WallMap.Clear();

        foreach (WalkerUnit walker in GetChildren())
        {
            walker.CalcPaht();
        }

        BuildMap();
    }

    public void BuildMap()
    {
        floorSet.Clear();

        foreach (WalkerUnit walker in GetChildren())
        {
            foreach (var pos in walker.Ground)
            {
                if (!floorSet.Contains(pos)) floorSet.Add(pos);
            }
        }

        GroundMap.SetCellsTerrainConnect(floorSet, 0, 0);
            
        Godot.Collections.Array<Godot.Vector2I> Walls = [];

        for (int x = -MapLength; x < MapLength; x++)
        {
            for (int y = -MapLength; y < MapLength; y++)
            {
                var location = new Vector2I(x, y);
                if (!floorSet.Contains(location))
                {
                    WallMap.SetCell(location, 5, new Vector2I(12, 6));
                    UnderGround.SetCell(location, 5, new Vector2I(1,1));

                    //Walls.Add(location);
                }
                 
            }
        }

        //WallMap.SetCellsTerrainConnect(Walls, 0, 1);

        player.GlobalPosition = floorSet[floorSet.Count - 1] * 32 + new Vector2(16,16);
        main.walls = WallMap;

        Eventbus.TriggerSpawnEnemies(GroundMap);
    }

    public override void _Input(InputEvent input)
    {
        if (input.IsActionPressed("space"))
        {
            GenerateMap();
        }
    }

    public void Explosion(int size, Vector2 position)
    {
        Vector2I centerPos = GroundMap.LocalToMap(position);

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
