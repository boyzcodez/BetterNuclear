using Godot;

public sealed class WallGrid
{
    // Maps "world -> tilemap local"
    private Transform2D _worldToLocal;
    private Transform2D _localToWorld;

    private Vector2I _tileSize;         // in local pixels
    private Rect2I _usedRect;           // in tile coords (min + size)
    private bool[] _solid;              // baked occupancy

    public int Width  => _usedRect.Size.X;
    public int Height => _usedRect.Size.Y;
    public Vector2I Origin => _usedRect.Position; // min cell

    public void RebuildFrom(TileMapLayer walls)
    {
        // Cache transforms (no calling ToLocal/ToGlobal in hot paths)
        _localToWorld = walls.GlobalTransform;
        _worldToLocal = walls.GlobalTransform.AffineInverse();

        _tileSize = walls.TileSet.TileSize;

        // Used rect is *tile coords*, not pixels
        _usedRect = walls.GetUsedRect();

        // If your walls layer is sparse but huge, you may want a fixed bounds instead.
        int w = Mathf.Max(1, _usedRect.Size.X);
        int h = Mathf.Max(1, _usedRect.Size.Y);

        _solid = new bool[w * h];

        // Bake: one-time cost
        // Note: this assumes "SourceId != -1 means wall". Adjust if you use multiple sources.
        for (int y = 0; y < h; y++)
        {
            int cy = _usedRect.Position.Y + y;
            for (int x = 0; x < w; x++)
            {
                int cx = _usedRect.Position.X + x;
                int idx = x + y * w;
                _solid[idx] = walls.GetCellSourceId(new Vector2I(cx, cy)) != -1;
            }
        }
    }

    public Vector2 WorldToLocal(Vector2 world) => _worldToLocal * world;
    public Vector2 LocalToWorld(Vector2 local) => _localToWorld * local;

    public bool IsWallCell(Vector2I cell)
    {
        int x = cell.X - _usedRect.Position.X;
        int y = cell.Y - _usedRect.Position.Y;

        if ((uint)x >= (uint)_usedRect.Size.X) return false;
        if ((uint)y >= (uint)_usedRect.Size.Y) return false;

        return _solid[x + y * _usedRect.Size.X];
    }

    public Vector2I LocalToCellFast(Vector2 localPos)
    {
        // Orthogonal grid assumption (most top-down TileMaps).
        // If you use isometric/hex, keep LocalToMap for correctness.
        return new Vector2I(
            Mathf.FloorToInt(localPos.X / _tileSize.X),
            Mathf.FloorToInt(localPos.Y / _tileSize.Y)
        );
    }

    public Vector2 LocalDirToWorldDir(Vector2 localDir)
    {
        // Transform direction with basis only (no translation).
        // GlobalTransform * vector includes translation, so we do basis manually:
        Vector2 worldDir = _localToWorld.BasisXform(localDir);
        float lenSq = worldDir.LengthSquared();
        if (lenSq <= 1e-12f) return Vector2.Zero;
        return worldDir / Mathf.Sqrt(lenSq);
    }
}
