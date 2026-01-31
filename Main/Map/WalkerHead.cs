using Godot;
using System.Collections.Generic;

public partial class WalkerHead : Node2D
{
    const int VIEW_WIDTH = 40;
    const int VIEW_HEIGHT = 30;
    const int MAP_PADDING = 10;

    int width = VIEW_WIDTH + MAP_PADDING * 2;
    int height = VIEW_HEIGHT + MAP_PADDING * 2;

    public enum Dirs { Left, Right, Up, Down }

    [Export] public int WalkerAmount = 6;
    [Export] public int PathLength = 100;

    [Export] public int WallPadding = 3;

    [Export] public TileMapLayer UnderGround;
    [Export] public TileMapLayer GroundMap;
    [Export] public TileMapLayer WallMap;

    [Export] public ItemResource dust;

    private Rect2I mapBounds;
    private Rect2I destructionBounds;
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

        main.walls = WallMap;
        main.ground = GroundMap;

        Eventbus.GenerateMap += GenerateMap;
        Eventbus.Explosion += Explosion;
        Eventbus.Reset += GenerateMap;

        GenerateMap();
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


    private void BuildPaths()
    {
        mapBounds = new Rect2I(
            -width / 2,
            -height / 2,
            width,
            height
        );

        destructionBounds = new Rect2I(
        mapBounds.Position + Vector2I.One * WallPadding,
        mapBounds.Size - Vector2I.One * WallPadding * 2
        );

        for (int i = 0; i < WalkerAmount; i++)
            RunWalker();
    }

    private void RunWalker()
    {
        Vector2I pos = Vector2I.Zero;
        floorSet.Add(pos);

        for (int i = 0; i < PathLength; i++)
        {
            pos += Directions[GD.RandRange(0, 3)];

            if (!mapBounds.HasPoint(pos))
                continue;

            floorSet.Add(pos);
        }
    }


    private void BuildFloors()
    {
        var floorArray = new Godot.Collections.Array<Vector2I>(floorSet);
        GroundMap.SetCellsTerrainConnect(floorArray, 0, 0);
    }


    private void BuildWalls()
    {
        Godot.Collections.Array<Vector2I> walls = new();

        for (int x = mapBounds.Position.X; x < mapBounds.End.X; x++)
        {
            for (int y = mapBounds.Position.Y; y < mapBounds.End.Y; y++)
            {
                Vector2I pos = new(x, y);
                if (!floorSet.Contains(pos))
                    walls.Add(pos);
            }
        }

        // use this when wall terrain set is ready
        //WallMap.SetCellsTerrainConnect(walls, 0, 1);

        foreach (var pos in walls)
        {
            WallMap.SetCell(pos, 5, new Vector2I(12, 6));  // remove later
            UnderGround.SetCell(pos, 5, new Vector2I(8, 5));
        }
            
    }


    private void SpawnPlayerAndEnemies()
    {
        Vector2I lastTile = default;
        foreach (var tile in floorSet)
            lastTile = tile;

        Vector2 spawnPos = GroundMap.MapToLocal(lastTile);

        player.GlobalPosition = spawnPos;

        main.RebuildWallCacheFromTilemap();

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
            Eventbus.TriggerGenerateMap();
    }

    // Explosion doesnt take into account that the map needs to be updated
    // once a wall is gone, there is no ground underneath, hence the map flow field doesnt
    // have a direction for said spot
    public void Explosion(float radius, Vector2 position, DamageData sm)
    {
        int size = Mathf.RoundToInt(radius / 25f);

        Vector2I centerPos = GroundMap.LocalToMap(GroundMap.ToLocal(position));

        int size2 = size * size;

        for (int x = -size; x <= size; x++)
        for (int y = -size; y <= size; y++)
        {
            if (x * x + y * y > size2) continue;

            Vector2I wallPos = centerPos + new Vector2I(x, y);

            if (!destructionBounds.HasPoint(wallPos))
                continue;

            // Fast check via Main (no GetCellSourceId in the loop)
            if (!main.IsWallCell(wallPos))
                continue;

            DestroyWall(wallPos);

            Vector2 dustPos = WallMap.ToGlobal(WallMap.MapToLocal(wallPos));
            Eventbus.TriggerSpawnItem(dust.Id, dustPos);
        }
    }

    public void DestroyWall(Vector2I pos)
    {
        WallMap.EraseCell(pos);
        main.NotifyWallRemoved(pos);   // keep Main cache in sync
    }
}
