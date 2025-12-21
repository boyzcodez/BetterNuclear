using Godot;
using System.Collections.Generic;

public partial class Main : Node2D
{
    public static Main Instance {get; private set;}

    private Dictionary<Vector2I, Vector2I> flowField = new();
    private int width = 200;
    private int height = 200;
    private static readonly Vector2I[] Directions =
    {
        Vector2I.Right,
        Vector2I.Left,
        Vector2I.Up,
        Vector2I.Down
    };
    private Vector2I previousSpot;

    public SpatialGrid grid;
    public TileMapLayer walls;
    public TileMapLayer ground;

    public List<Bullet> bullets = new();
    public List<Hurtbox> hurtboxes = new();

    public override void _Ready()
    {
        Instance = this;
        grid = new SpatialGrid(96f);
    }

    // this is for bounce logic
    public bool IsWallAt(Vector2 worldpos)
    {
        Vector2I cell = walls.LocalToMap(walls.ToLocal(worldpos));

        // SourceId == -1 means empty
        return walls.GetCellSourceId(cell) != -1;
    }

    public Vector2I WorldToCell(Vector2 worldPos)
    {
        return walls.LocalToMap(walls.ToLocal(worldPos));
    }
    public bool IsWallCell(Vector2I cell)
    {
        return walls.GetCellSourceId(cell) != -1;
    }
    public Vector2 CellToWorldCenter(Vector2I cell)
    {
        Vector2 localPos = ground.MapToLocal(cell);
        Vector2 tileSize = ground.TileSet.TileSize;
        return ground.ToGlobal(localPos + tileSize * 0.5f);
    }



    private bool ShouldUpdateFlow(Vector2 pos)
    {
        Vector2I cell = WorldToCell(pos);

        if (cell == previousSpot)
        {
            return false;
        }
        else
        {
            previousSpot = cell;
            return true;
        }
    }
    // this is for flow field
    public void GenerateTowardsTarget(Vector2 targetWorldPosition)
    {
        if (!ShouldUpdateFlow(targetWorldPosition)) return;

        flowField.Clear();

        Vector2I targetCell = ground.LocalToMap(
            ground.ToLocal(targetWorldPosition)
        );

        Queue<Vector2I> queue = new();
        Dictionary<Vector2I, Vector2I> cameFrom = new();

        queue.Enqueue(targetCell);
        cameFrom[targetCell] = targetCell;

        while (queue.Count > 0)
        {
            Vector2I current = queue.Dequeue();

            foreach (Vector2I dir in Directions)
            {
                Vector2I neighbor = current + dir;

                if (cameFrom.ContainsKey(neighbor))
                    continue;

                if (!IsWalkable(neighbor))
                    continue;

                cameFrom[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        // Build flow directions
        foreach (var pair in cameFrom)
        {
            Vector2I cell = pair.Key;
            Vector2I next = pair.Value;

            if (cell == targetCell)
            {
                flowField[cell] = Vector2I.Zero;
                continue;
            }

            Vector2I delta = next - cell;
            flowField[cell] = delta;
        }
    }
    public Vector2 GetDirection(Vector2 worldPosition)
    {
        Vector2I cell = ground.LocalToMap(
            ground.ToLocal(worldPosition)
        );

        if (flowField.TryGetValue(cell, out Vector2I dir))
        {
            return dir;
        }

        return Vector2.Zero;
    }
    // GetSmooth is GPT stuff so careful
    public Vector2 GetSmoothDirection(Vector2 worldPos)
    {
        Vector2 local = ground.ToLocal(worldPos);
        Vector2 tileSize = ground.TileSet.TileSize;
        Vector2 gridPos = local / tileSize;

        Vector2I c00 = new(Mathf.FloorToInt(gridPos.X), Mathf.FloorToInt(gridPos.Y));
        Vector2I c10 = c00 + Vector2I.Right;
        Vector2I c01 = c00 + Vector2I.Down;
        Vector2I c11 = c00 + new Vector2I(1, 1);

        Vector2 f = gridPos - c00;

        Vector2 d00 = flowField.GetValueOrDefault(c00);
        Vector2 d10 = flowField.GetValueOrDefault(c10);
        Vector2 d01 = flowField.GetValueOrDefault(c01);
        Vector2 d11 = flowField.GetValueOrDefault(c11);

        Vector2 dx0 = d00.Lerp(d10, f.X);
        Vector2 dx1 = d01.Lerp(d11, f.X);

        return dx0.Lerp(dx1, f.Y);
    }
    private bool IsWalkable(Vector2I cell)
    {
        // If there's a wall tile here, it's blocked
        if (walls.GetCellSourceId(cell) != -1)
            return false;

        // Must have ground
        return ground.GetCellSourceId(cell) != -1;
    }


    // this is for hit detection
    public override void _PhysicsProcess(double delta)
    {
        grid.Clear();

        foreach (var hurtbox in hurtboxes)
        {
            if (!hurtbox.active) continue;
            grid.Insert(hurtbox);
        } 
        foreach (var bullet in bullets)
        {
            if (!bullet.Active) continue;
            grid.Insert(bullet);
        } 

        HandleBulletCollision();
    }


    private void HandleBulletCollision()
    {
        foreach (var bullet in bullets)
        {
            if (!bullet.Active) continue;

            foreach (var obj in grid.QueryNearby(bullet.GlobalPosition))
            {
                if (obj is not Hurtbox hurtbox) continue;

                if (bullet.CollisionLayer != obj.CollisionLayer) continue;

                float r = bullet.Radius + hurtbox.Radius;
                if (bullet.GlobalPosition.DistanceSquaredTo(hurtbox.GlobalPosition) <= r * r)
                {
                    hurtbox.TakeDamage(1);
                    bullet.Deactivate();
                    break;
                }
            }
        }
    }

}
