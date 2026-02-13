using Godot;
using System.Collections.Generic;

public enum Generators
{
    Walker,
    Dungeon,
    Something
}

public partial class MapGenerator : Node2D
{
    private const int ShadowSourceId = 1;
    private static readonly Vector2I ShadowAtlasCoords = new Vector2I(42, 18);
    private static readonly Vector2I ShadowOffset = new(0, 1);

    [Export] public MapData Map;

    [Export] private WalkerGenerator walker;
    [Export] private Node dungeon;
    [Export] private Node something; // later

    [Export] public TileMapLayer ShadowMap;
    [Export] public TileMapLayer UnderGround;
    [Export] public TileMapLayer GroundMap;
    [Export] public TileMapLayer WallMap;

    [Export] public ItemResource dust;

    private Rect2I _mapBounds;
    private Rect2I _destructionBounds;

    private readonly HashSet<Vector2I> _floorSet = new();
    private readonly HashSet<Vector2I> _wallSet = new();

    private Player _player;
    private Main _main;

    public override void _Ready()
    {
        Map ??= new MapData();

        _main = GetTree().GetFirstNodeInGroup("Main") as Main;
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;

        // keep Main pointers centralized here now
        if (_main != null)
        {
            _main.walls = WallMap;
            _main.ground = GroundMap;
        }

        Eventbus.GenerateMap += GenerateMap;
        Eventbus.Reset += GenerateMap;
        Eventbus.Explosion += Explosion;

        GenerateMap();
    }

    public override void _ExitTree()
    {
        Eventbus.GenerateMap -= GenerateMap;
        Eventbus.Reset -= GenerateMap;
        Eventbus.Explosion -= Explosion;
    }

    public void GenerateMap()
    {
        _floorSet.Clear();
        _wallSet.Clear();

        GroundMap.Clear();
        WallMap.Clear();
        UnderGround.Clear();
        ShadowMap.Clear();

        MapGenResult result = RunSelectedGenerator(Map.generator);

        _mapBounds = result.MapBounds;
        _destructionBounds = result.DestructionBounds;

        _floorSet.UnionWith(result.Floors);

        BuildFloors();
        BuildWalls_FullFillLikeBefore();

        BuildAllShadows();

        SpawnPlayerAndEnemies(result.SpawnTile);
    }

    private MapGenResult RunSelectedGenerator(Generators key)
    {
        IMapAlgorithm algo = key switch
        {
            Generators.Walker => walker as IMapAlgorithm,
            Generators.Dungeon => dungeon as IMapAlgorithm,
            Generators.Something => something as IMapAlgorithm,
            _ => walker as IMapAlgorithm
        };

        if (algo == null)
        {
            GD.PushError($"No generator found/assigned for {key}. Falling back to Walker.");
            algo = walker;
        }

        return algo.Generate(Map);
    }

    private void BuildFloors()
    {
        var floorArray = new Godot.Collections.Array<Vector2I>(_floorSet);
        GroundMap.SetCellsTerrainConnect(floorArray, Map.GroundTerrainSet, Map.GroundTerrain, false);
    }

    private void BuildWalls_FullFillLikeBefore()
    {
        for (int x = _mapBounds.Position.X; x < _mapBounds.End.X; x++)
        {
            for (int y = _mapBounds.Position.Y; y < _mapBounds.End.Y; y++)
            {
                Vector2I pos = new(x, y);
                if (!_floorSet.Contains(pos))
                    _wallSet.Add(pos);
            }
        }

        var wallArray = new Godot.Collections.Array<Vector2I>(_wallSet);
        WallMap.SetCellsTerrainConnect(wallArray, Map.WallTerrainSet, Map.WallTerrain, false);

        foreach (var pos in _wallSet)
            UnderGround.SetCell(pos, Map.UnderGroundSourceId, Map.UnderGroundAtlasCoords);
    }
    
    private void BuildAllShadows()
    {
        if (ShadowMap == null) return;

        ShadowMap.Clear();
        foreach (var wallPos in _wallSet)
            RefreshShadowAt(wallPos + ShadowOffset);
    }
    private bool HasAnyGround(Vector2I pos)
    {
        // "Ground" can be either the autotiled floor OR the underground fill
        bool hasFloor = GroundMap != null && GroundMap.GetCellSourceId(pos) != -1;
        bool hasUnderground = UnderGround != null && UnderGround.GetCellSourceId(pos) != -1;
        return hasFloor || hasUnderground;
    }

    private void RefreshShadowAt(Vector2I shadowPos)
    {
        if (ShadowMap == null || WallMap == null) return;

        bool hasGround = HasAnyGround(shadowPos);
        bool hasWallAbove = WallMap.GetCellSourceId(shadowPos - ShadowOffset) != -1;

        if (hasGround && hasWallAbove)
            ShadowMap.SetCell(shadowPos, ShadowSourceId, ShadowAtlasCoords);
        else
            ShadowMap.EraseCell(shadowPos);
    }
    private void RefreshExplosionTopShadowLine(Vector2I centerPos, int size)
    {
        if (ShadowMap == null || GroundMap == null || WallMap == null) return;

        int yTopInside = centerPos.Y - size; // top row *inside* the explosion square

        for (int x = centerPos.X - size; x <= centerPos.X + size; x++)
        {
            Vector2I shadowPos = new(x, yTopInside);
            RefreshShadowAt(shadowPos); // checks wall above + ground here, then set/erase shadow
        }
    }

    private void SpawnPlayerAndEnemies(Vector2I spawnTile)
    {
        if (_player == null || _main == null)
            return;

        Vector2 spawnPos = GroundMap.MapToLocal(spawnTile);
        _player.GlobalPosition = spawnPos;

        _main.RebuildWallCacheFromTilemap();

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
        if (_main == null) return;

        int size = Mathf.RoundToInt(radius / 50f);
        Vector2I centerPos = GroundMap.LocalToMap(GroundMap.ToLocal(position));

        for (int x = -size; x <= size; x++)
        for (int y = -size; y <= size; y++)
        {
            Vector2I wallPos = centerPos + new Vector2I(x, y);

            if (!_destructionBounds.HasPoint(wallPos))
                continue;

            // Fast check via Main cache
            if (!_main.IsWallCell(wallPos))
                continue;

            DestroyWall(wallPos);

            if (dust != null)
            {
                Vector2 dustPos = WallMap.ToGlobal(WallMap.MapToLocal(wallPos));
                Eventbus.TriggerSpawnItem(dust.Id, dustPos);
            }
        }

        RefreshExplosionTopShadowLine(centerPos, size);
    }

    public void DestroyWall(Vector2I pos)
    {
        WallMap.SetCellsTerrainConnect([pos], Map.WallTerrainSet, -1, false);
        _main?.NotifyWallRemoved(pos);
        RefreshShadowAt(pos + ShadowOffset);
    }
}
