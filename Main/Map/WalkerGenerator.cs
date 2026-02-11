using Godot;
using System.Collections.Generic;

public partial class WalkerGenerator : Node, IMapAlgorithm
{
    private static readonly Vector2I[] Directions =
    {
        Vector2I.Left,
        Vector2I.Right,
        Vector2I.Up,
        Vector2I.Down
    };

    public MapGenResult Generate(MapData map)
    {
        var floorSet = new HashSet<Vector2I>();

        int e = map.InitialExtent;
        int size = e * 2 + 1;
        Rect2I mapBounds = new Rect2I(-e, -e, size, size);

        for (int i = 0; i < map.WalkerAmount; i++)
            RunWalker(map, mapBounds, floorSet);

        (mapBounds, Rect2I destructionBounds) = RecomputeBoundsFromFloors_Tight(map, floorSet);

        Vector2I spawnTile = Vector2I.Zero;
        foreach (var t in floorSet) spawnTile = t;

        return new MapGenResult(floorSet, mapBounds, destructionBounds, spawnTile);
    }

    private void RunWalker(MapData map, Rect2I mapBounds, HashSet<Vector2I> floorSet)
    {
        Vector2I pos = Vector2I.Zero;
        floorSet.Add(pos);

        for (int i = 0; i < map.PathLength; i++)
        {
            pos += Directions[GD.RandRange(0, 3)];

            if (!mapBounds.HasPoint(pos))
                continue;

            floorSet.Add(pos);
        }
    }

    private (Rect2I mapBounds, Rect2I destructionBounds) RecomputeBoundsFromFloors_Tight(MapData map, HashSet<Vector2I> floorSet)
    {
        if (floorSet.Count == 0)
        {
            // fallback to something sane
            var fallback = new Rect2I(-8, -8, 17, 17);
            return (fallback, fallback);
        }

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var p in floorSet)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        int pad = map.TightOuterPadding + map.TerrainSafetyMargin;

        Vector2I pos0 = new(minX - pad, minY - pad);
        Vector2I end = new(maxX + pad + 1, maxY + pad + 1); // End is exclusive
        Rect2I bounds = new Rect2I(pos0, end - pos0);

        int inset = Mathf.Min(map.TightDestructionInset, Mathf.Min(bounds.Size.X / 2, bounds.Size.Y / 2));
        Rect2I destructionBounds = new Rect2I(
            bounds.Position + Vector2I.One * inset,
            bounds.Size - Vector2I.One * inset * 2
        );

        return (bounds, destructionBounds);
    }
}
