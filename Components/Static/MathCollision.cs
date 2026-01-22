using Godot;

public static class MathCollision
{
    // Push a circle out of overlapping wall tiles and optionally slide velocity along normals.
    public static void ResolveCircleVsWalls(ref Vector2 pos, ref Vector2 vel, float radius, float slop = 0.001f, int iterations = 3)
    {
        var main = Main.Instance;
        Vector2 tileSize = main.WallTileSize();

        // Check a small neighborhood around the circle
        int rx = Mathf.CeilToInt(radius / tileSize.X) + 1;
        int ry = Mathf.CeilToInt(radius / tileSize.Y) + 1;

        float r2 = radius * radius;

        // Multiple passes helps when you overlap more than one tile (corners, tight gaps)
        for (int pass = 0; pass < iterations; pass++)
        {
            bool anyPush = false;

            Vector2I baseCell = main.WorldToCell(pos);

            for (int y = -ry; y <= ry; y++)
            for (int x = -rx; x <= rx; x++)
            {
                Vector2I cell = baseCell + new Vector2I(x, y);
                if (!main.IsWallCell(cell))
                    continue;

                Rect2 rect = main.WallCellWorldRect(cell);

                // Closest point on the rect to the circle center
                float cx = Mathf.Clamp(pos.X, rect.Position.X, rect.Position.X + rect.Size.X);
                float cy = Mathf.Clamp(pos.Y, rect.Position.Y, rect.Position.Y + rect.Size.Y);
                Vector2 closest = new Vector2(cx, cy);

                Vector2 delta = pos - closest;
                float dist2 = delta.LengthSquared();

                if (dist2 >= r2)
                    continue;

                // Compute push-out normal
                Vector2 n;
                float dist;

                if (dist2 > 1e-10f)
                {
                    dist = Mathf.Sqrt(dist2);
                    n = delta / dist;
                }
                else
                {
                    // Center is exactly on/inside the rect; choose the smallest-penetration axis
                    float left   = Mathf.Abs(pos.X - rect.Position.X);
                    float right  = Mathf.Abs((rect.Position.X + rect.Size.X) - pos.X);
                    float top    = Mathf.Abs(pos.Y - rect.Position.Y);
                    float bottom = Mathf.Abs((rect.Position.Y + rect.Size.Y) - pos.Y);

                    float m = left;
                    n = new Vector2(-1, 0);

                    if (right < m) { m = right; n = new Vector2( 1, 0); }
                    if (top   < m) { m = top;   n = new Vector2( 0,-1); }
                    if (bottom< m) { m = bottom;n = new Vector2( 0, 1); }

                    dist = 0f;
                }

                float push = (radius - dist) + slop;
                pos += n * push;
                anyPush = true;

                // Slide: remove velocity component INTO the wall
                float vn = vel.Dot(n);
                if (vn < 0f)
                    vel -= n * vn;
            }

            if (!anyPush)
                break;
        }
    }

    // Substep movement to avoid tunneling/snags at higher speeds
    public static void MoveCircle(ref Vector2 pos, ref Vector2 vel, float radius, float delta)
    {
        var main = Main.Instance;
        Vector2 tileSize = main.WallTileSize();

        Vector2 move = vel * delta;
        float len = move.Length();

        // Step size ~ half a tile is a good cheap default
        float stepSize = Mathf.Min(tileSize.X, tileSize.Y) * 0.45f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(len / stepSize));

        Vector2 step = move / steps;

        for (int i = 0; i < steps; i++)
        {
            pos += step;
            ResolveCircleVsWalls(ref pos, ref vel, radius);
        }
    }
}
