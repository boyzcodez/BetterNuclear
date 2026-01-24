using Godot;
using System.Collections.Generic;

public partial class Main : Node2D
{
    // name: into dimension death
    public static Main Instance {get; private set;}

    public SpatialGrid grid;
    public TileMapLayer walls;
    public TileMapLayer ground;

    public WallGrid wallGrid {get;private set;} = new WallGrid();
    public List<Hurtbox> hurtboxes = new();

    private readonly HashSet<Vector2I> _wallCells = new();

    public override void _Ready()
    {
        Instance = this;
        grid = new SpatialGrid(96f);

        Eventbus.Explosion += DamageHurtboxesInArea;
    }

    public void RebuildWallCacheFromTilemap()
    {
        _wallCells.Clear();

        foreach (var cell in walls.GetUsedCells())
            _wallCells.Add(cell);
    }
    public void NotifyWallRemoved(Vector2I cell) => _wallCells.Remove(cell);
    public void NotifyWallPlaced(Vector2I cell)  => _wallCells.Add(cell);
    
    // IMPORTANT: route all wall edits through these so the cache stays correct
    public void SetWallCell(Vector2I cell, int sourceId, Vector2I atlas = default, int alt = 0)
    {
        walls.SetCell(cell, sourceId, atlas, alt);
        if (sourceId == -1) _wallCells.Remove(cell);
        else _wallCells.Add(cell);
    }

    // Used everywhere in gameplay code
    public bool IsWallCell(Vector2I cell) => _wallCells.Contains(cell);

    public bool IsWallAt(Vector2 worldPos)
    {
        Vector2I cell = walls.LocalToMap(walls.ToLocal(worldPos));
        return _wallCells.Contains(cell);
    }

    public Vector2I WorldToCell(Vector2 worldPos)
    {
        return walls.LocalToMap(walls.ToLocal(worldPos));
    }    
    public Rect2 WallCellWorldRect(Vector2I cell)
    {
        Vector2 tileSize = walls.TileSet.TileSize;

        // In Godot 4, MapToLocal returns the cell's local position (typically the cell center).
        // We convert that to global, then build a rect around it.
        Vector2 localCellPos = walls.MapToLocal(cell);
        Vector2 worldCenter = walls.ToGlobal(localCellPos);

        Vector2 topLeft = worldCenter - tileSize * 0.5f;
        return new Rect2(topLeft, tileSize);
    }
    public Vector2 WallTileSize()
    {
        return walls.TileSet.TileSize;
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
                    hurtbox.TakeDamage(damageData, direction);
                }
            }
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        grid.Clear();

        for (int i = 0; i < hurtboxes.Count; i++)
        {
            var hurtbox = hurtboxes[i];
            if (!hurtbox.active) continue;
            grid.Insert(hurtbox);
        }

        var activeBullets = BulletPool.Instance.GetActiveSnapshot();

        for (int i = 0; i < activeBullets.Count; i++)
        {
            var bullet = activeBullets[i];
            if (!bullet.Active) continue; // optional if pool only returns active
            grid.Insert(bullet);
            bullet.Update(delta);
        }

        HandleBulletCollision(activeBullets);
    }


    private void HandleBulletCollision(IReadOnlyList<ModularBullet> activeBullets)
    {
        for (int i = 0; i < activeBullets.Count; i++)
        {
            var bullet = activeBullets[i];
            if (!bullet.Active) continue;

            foreach (var obj in grid.QueryNearby(bullet.GlobalPosition))
            {
                if (obj is not Hurtbox hurtbox) continue;
                if (bullet.Layer != obj.CollisionLayer) continue;

                
                float r = bullet.Radius + hurtbox.Radius;
                if (bullet.GlobalPosition.DistanceSquaredTo(hurtbox.GlobalPosition) > r * r)
                    continue;

                
                var bcol = (ICollidable)bullet;
                var hcol = (ICollidable)hurtbox;

                if (bcol.CollisionShape != null && hcol.CollisionShape != null)
                {
                    bool hit = bcol.CollisionShape.Collide(
                        bcol.CollisionXform,
                        hcol.CollisionShape,
                        hcol.CollisionXform
                    );

                    if (!hit) continue;
                }

                // HIT!
                if (hurtbox.Health > 0)
                {
                    hurtbox.TakeDamage(bullet.damageData, bullet.Velocity);

                    bullet.NotifyOnHit(hurtbox);

                    if (hurtbox.Health <= 0)
                    {
                        bullet.NotifyEnemyKilled(hurtbox);
                    }
                }
                break;
            }
        }
    }

}
