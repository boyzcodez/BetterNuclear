using Godot;
using System.Collections.Generic;

public partial class WalkerHead : Node2D
{
    public enum Dirs { Left, Right, Up, Down }

    [Export] public int WalkerAmount = 6;
    [Export] public int PathLength = 100;

    [Export] public TileMapLayer UnderGround;
    [Export] public TileMapLayer GroundMap;
    [Export] public TileMapLayer WallMap;

    private HashSet<Vector2I> floorSet = new();

    private Player player;
    private Main main;

    private static readonly Vector2I[] Directions =
    {
        Vector2I.Left,
        Vector2I.Right,
        Vector2I.Up,
        Vector2I.Down
    };

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
        UnderGround.Clear();

        BuildPaths();
        BuildFloors();
        BuildWalls();
        SpawnPlayerAndEnemies();
    }

    // ------------------------
    // PATH GENERATION
    // ------------------------
    private void BuildPaths()
    {
        for (int i = 0; i < WalkerAmount; i++)
            RunWalker();
    }

    private void RunWalker()
    {
        Vector2I position = (Vector2I)GlobalPosition;
        floorSet.Add(position);

        for (int i = 0; i < PathLength; i++)
        {
            position += Directions[GD.RandRange(0, 3)];
            floorSet.Add(position);
        }
    }

    // ------------------------
    // FLOOR TILEMAP
    // ------------------------
    private void BuildFloors()
    {
        var floorArray = new Godot.Collections.Array<Vector2I>(floorSet);
        GroundMap.SetCellsTerrainConnect(floorArray, 0, 0);
    }

    // ------------------------
    // WALL TILEMAP
    // ------------------------
    private void BuildWalls()
    {
        HashSet<Vector2I> wallSet = new();

        for (int x = -(PathLength + 50); x < PathLength; x++)
        {
            for (int y = -(PathLength + 50); y < PathLength; y++)
            {
                Vector2I spot = new Vector2I(x, y);

                if (!floorSet.Contains(spot)) wallSet.Add(spot);
            }
        }

        var walls = new Godot.Collections.Array<Vector2I>(wallSet);

        // this will be used when i have made a tileset and auto tiling for walls
        //WallMap.SetCellsTerrainConnect(walls, 0, 1);

        // setting up the ground below the walls here
        foreach (var pos in walls)
        {
            WallMap.SetCell(pos, 5, new Vector2I(12, 6));
            UnderGround.SetCell(pos, 5, new Vector2I(8, 5));
        }
            
    }

    // ------------------------
    // PLAYER & ENEMIES
    // ------------------------
    private void SpawnPlayerAndEnemies()
    {
        Vector2I lastTile = default;
        foreach (var tile in floorSet)
            lastTile = tile;

        Vector2 spawnPos = GroundMap.MapToLocal(lastTile);

        player.GlobalPosition = spawnPos;

        main.walls = WallMap;
        main.ground = GroundMap;

        if (Eventbus.gameOn)
            Eventbus.TriggerSpawnEnemies(GroundMap);
    }

    // ------------------------
    // INPUT (DEBUG)
    // ------------------------
    public override void _Input(InputEvent e)
    {
        Eventbus.gameOn = true;

        if (e.IsActionPressed("space"))
            GenerateMap();
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
