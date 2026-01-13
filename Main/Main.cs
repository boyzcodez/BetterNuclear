using Godot;
using System.Collections.Generic;

public partial class Main : Node2D
{
    public static Main Instance {get; private set;}

    public SpatialGrid grid;
    public TileMapLayer walls;
    public TileMapLayer ground;

    public WallGrid wallGrid {get;private set;} = new WallGrid();

    public List<ModularBullet> bullets = new();
    public List<Hurtbox> hurtboxes = new();

    public override void _Ready()
    {
        Instance = this;
        grid = new SpatialGrid(96f);
        wallGrid.RebuildFrom(walls);

        Eventbus.Explosion += DamageHurtboxesInArea;
    }

    public void UpdateMap(TileMapLayer wal, TileMapLayer grou)
    {
        walls = wal;
        ground = grou;

        wallGrid.RebuildFrom(walls);
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


    // this is for hit detection

    public void DamageHurtboxesInArea(float radius, Vector2 position, DamageData damageData)
    {
        float radiusSq = radius * radius;

        foreach (var hurtbox in hurtboxes)
        {
            if (!hurtbox.active) continue;
            
            if (position.DistanceSquaredTo(hurtbox.GlobalPosition) <= radiusSq)
            {
                Vector2 direction = (hurtbox.GlobalPosition - position).Normalized();

                if (hurtbox.Health > 0)
                {
                    hurtbox.TakeDamage(damageData.Damage, damageData.Knockback, direction);
                }
            }
        }
    }


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
            bullet.Update(delta);
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

                if (bullet.Layer != obj.CollisionLayer) continue;

                float r = bullet.Radius + hurtbox.Radius;
                if (bullet.GlobalPosition.DistanceSquaredTo(hurtbox.GlobalPosition) <= r * r)
                {
                    if (hurtbox.Health > 0)
                    {
                        hurtbox.TakeDamage(bullet.Damage, bullet.Knockback, bullet.Velocity);
                        foreach (var behavior in bullet.Behaviors)
                        {
                            behavior.OnHit(bullet, hurtbox);
                        }

                        if (hurtbox.Health <= 0)
                        {
                            foreach (var behavior in bullet.Behaviors)
                            {
                                behavior.OnKill(bullet, hurtbox);
                            }
                        }
                    }
                    
                    break;
                }
            }
        }
    }

}
