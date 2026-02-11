using Godot;
using System.Collections.Generic;

public partial class WalkerHead : Node2D
{
    public enum Dirs { Left, Right, Up, Down }

    [Export] public MapData Map;

    [Export] public TileMapLayer UnderGround;
    [Export] public TileMapLayer GroundMap;
    [Export] public TileMapLayer WallMap;

    [Export] public ItemResource dust;

    private Rect2I mapBounds;
    private Rect2I destructionBounds;
    private readonly HashSet<Vector2I> floorSet = new();
    private readonly HashSet<Vector2I> wallSet = new();

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
        // If resource not assigned
        Map ??= new MapData();

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
        wallSet.Clear();

        GroundMap.Clear();
        WallMap.Clear();
        UnderGround.Clear();

        BuildPaths();
        RecomputeBoundsFromFloors_Tight();

        BuildFloors();

        BuildWalls();

        SpawnPlayerAndEnemies();
    }

    private void BuildPaths()
    {
        int e = Map.InitialExtent;

        int size = e * 2 + 1;

        mapBounds = new Rect2I(-e, -e, size, size);

        destructionBounds = new Rect2I(
            mapBounds.Position + Vector2I.One * Map.WallPadding,
            mapBounds.Size - Vector2I.One * Map.WallPadding * 2
        );

        for (int i = 0; i < Map.WalkerAmount; i++)
            RunWalker();
    }
    private void RecomputeBoundsFromFloors_Tight()
    {
        if (floorSet.Count == 0)
            return;

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var p in floorSet)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        int pad = Map.TightOuterPadding + Map.TerrainSafetyMargin;

        Vector2I pos = new(minX - pad, minY - pad);
        Vector2I end = new(maxX + pad + 1, maxY + pad + 1); // +1 because End is exclusive
        mapBounds = new Rect2I(pos, end - pos);

        int inset = Mathf.Min(Map.TightDestructionInset, Mathf.Min(mapBounds.Size.X / 2, mapBounds.Size.Y / 2));

        destructionBounds = new Rect2I(
            mapBounds.Position + Vector2I.One * inset,
            mapBounds.Size - Vector2I.One * inset * 2
        );
    }

    private void RunWalker()
    {
        Vector2I pos = Vector2I.Zero;
        floorSet.Add(pos);

        for (int i = 0; i < Map.PathLength; i++)
        {
            pos += Directions[GD.RandRange(0, 3)];

            // Important: keep behavior the same as before.
            // If out of bounds, we just don't add a tile, but we do NOT undo the step.
            if (!mapBounds.HasPoint(pos))
                continue;

            floorSet.Add(pos);
        }
    }

    private void BuildFloors()
    {
        var floorArray = new Godot.Collections.Array<Vector2I>(floorSet);
        GroundMap.SetCellsTerrainConnect(floorArray, Map.GroundTerrainSet, Map.GroundTerrain, false);
    }

    private void BuildWalls()
    {

        for (int x = mapBounds.Position.X; x < mapBounds.End.X; x++)
        {
            for (int y = mapBounds.Position.Y; y < mapBounds.End.Y; y++)
            {
                Vector2I pos = new(x, y);
                if (!floorSet.Contains(pos))
                    wallSet.Add(pos);
            }
        }

        // this is causing dips in frames
        var wallArray = new Godot.Collections.Array<Vector2I>(wallSet);
        WallMap.SetCellsTerrainConnect(wallArray, Map.WallTerrainSet, Map.WallTerrain, false);

        foreach (var pos in wallSet)
        {
            UnderGround.SetCell(pos, Map.UnderGroundSourceId, Map.UnderGroundAtlasCoords);
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
        
        Explosion(50f, spawnPos);
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("space"))
            Eventbus.TriggerGenerateMap();
    }

    public void Explosion(float radius, Vector2 position, DamageData sm = null)
    {
        int size = Mathf.RoundToInt(radius / 50f);

        Vector2I centerPos = GroundMap.LocalToMap(GroundMap.ToLocal(position));

        for (int x = -size; x <= size; x++)
        for (int y = -size; y <= size; y++)
        {
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
        WallMap.SetCellsTerrainConnect([pos], Map.WallTerrainSet, -1, false);
        main.NotifyWallRemoved(pos);   // keep Main cache in sync
    }
}
