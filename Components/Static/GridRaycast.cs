using Godot;

public static class GridRaycast
{
    private static Vector2 SafeNormalize(Vector2 v)
    {
        float lenSq = v.LengthSquared();
        if (lenSq <= 1e-12f) return Vector2.Zero;
        return v / Mathf.Sqrt(lenSq);
    }
    // Small helper used by DDA to find first grid boundary
    private static float IntBound(float s, float ds)
    {
        // Find t such that s + t*ds is an integer boundary
        if (ds > 0)
        {
            return (Mathf.Ceil(s) - s) / ds;
        }
        else if (ds < 0)
        {
            return (s - Mathf.Floor(s)) / -ds;
        }
        else
        {
            return float.PositiveInfinity;
        }
    }

    /// <summary>
    /// Cast a segment against the wall grid. Treats the bullet as a point (center).
    /// worldStart -> worldEnd. Returns true if a wall tile is entered.
    /// </summary>
    public static bool RaycastWalls(
        WallGrid grid,
        Vector2 worldStart,
        Vector2 worldEnd,
        out Vector2 hitWorldPos,
        out Vector2 hitNormal)
    {
        hitWorldPos = worldEnd;
        hitNormal = Vector2.Zero;

        // Convert to tilemap-local space once
        Vector2 a = grid.WorldToLocal(worldStart);
        Vector2 b = grid.WorldToLocal(worldEnd);

        Vector2 delta = b - a;
        if (delta == Vector2.Zero)
            return false;

        // Work in "tile units" (so grid lines are integers)
        // NOTE: relies on LocalToCellFast using the same tileSize.
        // If your tile sizes differ on X/Y, this still works because we normalize per axis below.
        // We do this by scaling positions into tile coords:
        // tileCoord = local / tileSize.
        // Since WallGrid keeps tileSize private, you can either expose it or pass it in.
        // Easiest: call LocalToCellFast + also compute tile coords by dividing using the same rule:
        // We'll reconstruct tile coords by sampling two cell conversions + local values:
        // (To keep code simple here, add a TileSize getter to WallGrid in your project.)
        //
        // For this snippet, assume you add:
        // public Vector2 TileSizeF => new Vector2(_tileSize.X, _tileSize.Y);

        // --- assume you add this property in WallGrid:
        // public Vector2 TileSizeF { get; private set; }
        // and set it in RebuildFrom: TileSizeF = (Vector2)_tileSize;

        // If you *don’t* want to add TileSizeF, just inline with your known tileSize (32,32).
        // I’ll use a fixed 32 here to match your current setup:
        const float tsx = 32f;
        const float tsy = 32f;

        Vector2 ta = new Vector2(a.X / tsx, a.Y / tsy);
        Vector2 tb = new Vector2(b.X / tsx, b.Y / tsy);

        Vector2 d = tb - ta;

        int x = Mathf.FloorToInt(ta.X);
        int y = Mathf.FloorToInt(ta.Y);

        int stepX = d.X > 0 ? 1 : (d.X < 0 ? -1 : 0);
        int stepY = d.Y > 0 ? 1 : (d.Y < 0 ? -1 : 0);

        float tMaxX = IntBound(ta.X, d.X);
        float tMaxY = IntBound(ta.Y, d.Y);

        float tDeltaX = stepX != 0 ? (1f / Mathf.Abs(d.X)) : float.PositiveInfinity;
        float tDeltaY = stepY != 0 ? (1f / Mathf.Abs(d.Y)) : float.PositiveInfinity;

        // If we start inside a wall, report immediately
        if (grid.IsWallCell(new Vector2I(x, y)))
        {
            hitWorldPos = worldStart;
            hitNormal = Vector2.Zero;
            return true;
        }

        // Walk until we pass t=1 (end of segment)
        float t = 0f;

        while (t <= 1f)
        {
            // Step to next cell boundary
            if (tMaxX < tMaxY)
            {
                x += stepX;
                t = tMaxX;
                tMaxX += tDeltaX;

                if (grid.IsWallCell(new Vector2I(x, y)))
                {
                    // Normal points opposite movement direction on that axis
                    hitNormal = grid.LocalDirToWorldDir(new Vector2(-stepX, 0));
                    Vector2 hitLocal = a + (b - a) * t;
                    hitWorldPos = grid.LocalToWorld(hitLocal);
                    return true;
                }
            }
            else
            {
                y += stepY;
                t = tMaxY;
                tMaxY += tDeltaY;

                if (grid.IsWallCell(new Vector2I(x, y)))
                {
                    hitNormal = grid.LocalDirToWorldDir(new Vector2(0, -stepY));
                    Vector2 hitLocal = a + (b - a) * t;
                    hitWorldPos = grid.LocalToWorld(hitLocal);
                    return true;
                }
            }
        }

        return false;
    }
}
