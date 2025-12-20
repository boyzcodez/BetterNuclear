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



    // this is for flow field
    public void GenerateTowardsTarget(Vector2 targetWorldPosition)
    {
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
