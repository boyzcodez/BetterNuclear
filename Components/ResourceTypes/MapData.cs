using Godot;

[GlobalClass]
public partial class MapData : Resource
{
    [Export] public Generators generator {get; set;} = Generators.Walker;
    [Export] private int _walkerAmount = 6;
    [Export] private int _pathLength = 100;

    public int WalkerAmount => Mathf.Max(1, _walkerAmount);
    public int PathLength   => Mathf.Max(1, _pathLength);

    [Export] private int _initialSafetyPadding = 8;
    public int InitialSafetyPadding => Mathf.Max(0, _initialSafetyPadding);

    [Export] private int _wallPadding = 2;
    public int WallPadding => Mathf.Max(0, _wallPadding);

    [Export] private int _tightOuterPadding = 3;
    [Export] private int _tightDestructionInset = 2;
    [Export] private int _terrainSafetyMargin = 1;

    public int TightOuterPadding      => Mathf.Max(0, _tightOuterPadding);
    public int TightDestructionInset  => Mathf.Max(0, _tightDestructionInset);
    public int TerrainSafetyMargin    => Mathf.Max(0, _terrainSafetyMargin);

    [Export] public int GroundTerrainSet = 0;
    [Export] public int GroundTerrain = 3;

    [Export] public int WallTerrainSet = 0;
    [Export] public int WallTerrain = 2;

    [Export] public int UnderGroundSourceId = 1;
    [Export] public Vector2I UnderGroundAtlasCoords = new(38, 15);

    public int InitialExtent =>
        PathLength
        + InitialSafetyPadding
        + TightOuterPadding
        + TerrainSafetyMargin
        + WallPadding;
}
