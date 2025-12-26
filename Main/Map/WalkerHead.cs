using Godot;
using System.Collections.Generic;

public partial class WalkerHead : Node2D
{
    public enum Dirs{LEFT,RIGHT,UP,DOWN}

    [Export] public int WalkerAmount = 6;
    [Export] public int PathLength = 100;

    [Export] public TileMapLayer UnderGround;
    [Export] public TileMapLayer GroundMap;
    [Export] public TileMapLayer WallMap;

    public Godot.Collections.Array<Vector2I> floorSet = [];
    private Player player;
    private Main main;

    public override void _Ready()
    {
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        GenerateMap();

        Eventbus.EnemiesKilled += GenerateMap;
    }

    public void GenerateMap()
    {
        floorSet.Clear();
        GroundMap.Clear();
        WallMap.Clear();

        BuildMap();
    }

    public void BuildMap()
    {

        for (int i = 0; i < WalkerAmount; i++)
        {
            Walker();
        }

        GroundMap.SetCellsTerrainConnect(floorSet, 0, 0);
            
        Godot.Collections.Array<Vector2I> Walls = [];

        for (int x = -(PathLength + 50); x < PathLength; x++)
        {
            for (int y = -(PathLength + 50); y < PathLength; y++)
            {
                var location = new Vector2I(x, y);
                if (!floorSet.Contains(location))
                {
                    WallMap.SetCell(location, 5, new Vector2I(12, 6));
                    UnderGround.SetCell(location, 5, new Vector2I(8,5));
                }
                 
            }
        }

        //WallMap.SetCellsTerrainConnect(Walls, 0, 1);

        var spot = floorSet[floorSet.Count - 1];
        var spawn = GroundMap.MapToLocal(spot);

        Explosion(2, spawn);
        player.GlobalPosition = spawn;

        main.walls = WallMap;
        main.ground = GroundMap;

        if (Eventbus.gameOn) Eventbus.TriggerSpawnEnemies(GroundMap);
    }

    public override void _Input(InputEvent input)
    {
        Eventbus.gameOn = true;

        if (input.IsActionPressed("space"))
        {
            GenerateMap();
        }
    }

    public void Walker()
    {
        List<int> PathSteps = new();
        for (int i = 0; i < PathLength; i++)
        {
            var stepsi = GD.RandRange(0, Dirs.GetNames(typeof(Dirs)).Length - 1);
            PathSteps.Add(stepsi);
        }

        Vector2I location = (Vector2I)GlobalPosition;

        foreach (int dir in PathSteps)
        {
            var ModifierDirection = Vector2I.Zero;

            switch (dir)
            {
                case 0:
                    ModifierDirection = Vector2I.Left;
                    break;
                case 1:
                    ModifierDirection = Vector2I.Right;
                    break;
                case 2:
                    ModifierDirection = Vector2I.Up;
                    break;
                case 3:
                    ModifierDirection = Vector2I.Down;
                    break;
            }
            location += ModifierDirection;

            if (!floorSet.Contains(location)) floorSet.Add(location);
        }
    }

    // Explosion doesnt take into account that the map needs to be updated
    // once a wall is gone, there is no ground underneath, hence the map flow field doesnt
    // have a direction for said spot
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
