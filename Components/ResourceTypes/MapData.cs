using Godot;

[GlobalClass]
public partial class MapData : Resource
{
    [Export] public int ViewWidth = 40;
    [Export] public int ViewHeight = 30;
    [Export] public int MapPadding = 10;

    [Export] public int WalkerAmount = 6;
    [Export] public int PathLength = 100;

    [Export] public int WallPadding = 5;

    // Floor terrain
    [Export] public int GroundTerrainSet = 0;
    [Export] public int GroundTerrain = 0;

    // Wall terrain (your new autotile terrain)
    [Export] public int WallTerrainSet = 0;
    [Export] public int WallTerrain = 2;

    // Underground still uses a specific tile (fine)
    [Export] public int UnderGroundSourceId = 5;
    [Export] public Vector2I UnderGroundAtlasCoords = new Vector2I(8, 5);

    public int Width => ViewWidth + MapPadding * 2;
    public int Height => ViewHeight + MapPadding * 2;
}
