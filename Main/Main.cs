using Godot;
using System.Collections.Generic;
using System.Numerics;

public partial class Main : Node2D
{
    public static Main Instance {get; private set;}

    private SpatialGrid grid;
    public TileMapLayer walls;

    public List<Bullet> bullets = new();
    public List<Hurtbox> hurtboxes = new();

    public override void _Ready()
    {
        Instance = this;
        grid = new SpatialGrid(96f);
    }

    // this is for bounce logic
    public bool IsWallAt(Godot.Vector2 worldpos)
    {
        Vector2I cell = walls.LocalToMap(walls.ToLocal(worldpos));

        // SourceId == -1 means empty
        return walls.GetCellSourceId(cell) != -1;
    }

    public Vector2I WorldToCell(Godot.Vector2 worldPos)
    {
        return walls.LocalToMap(walls.ToLocal(worldPos));
    }
    public bool IsWallCell(Vector2I cell)
    {
        return walls.GetCellSourceId(cell) != -1;
    }
    /// //
    
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
