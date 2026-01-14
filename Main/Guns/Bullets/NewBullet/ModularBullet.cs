using Godot;
using System.Collections.Generic;

public partial class ModularBullet : Sprite2D, ICollidable
{
    public Vector2 Velocity;
    public float Radius = 5f;
    public float LifeTime = 5f;
    public int Layer = 2; // 2 PlayerBullet, 1 EnemyBullet, etc

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => Layer;

    public float Damage;
    public float Knockback = 200f;
    public StringName PoolKey;
    public BulletPriority Priority;
    public float Speed = 400f;

    public readonly List<IBulletBehavior> Behaviors = new();

    
    public Vector2 PendingDisplacement { get; private set; }

    public Main main;
    public BulletPool pool;

    public bool Active = false;

    public override void _Ready()
    {
        pool = GetTree().GetFirstNodeInGroup("BulletPool") as BulletPool;
        main = GetTree().GetFirstNodeInGroup("Main") as Main;

        Visible = false;
    }

    public void Update(double delta)
    {
        Vector2 displacement = PendingDisplacement;
        PendingDisplacement = Vector2.Zero;

        MoveWithGridRay(displacement);

        foreach (var behavior in Behaviors) behavior.OnUpdate(this, (float)delta);

        LifeTime -= (float)delta;
        if (LifeTime <= 0) Deactivate();   
    }

    private void NotifyWallHit(Vector2 normal)
    {
        foreach (var b in Behaviors) b.OnWallHit(this, normal);
    }

    public void Activate(
        Vector2 position,
        Vector2 velocity,
        float lifetime,
        float damage,
        StringName poolKey,
        int ColLayer,
        BulletPriority priority,
        IEnumerable<IBulletBehavior> behaviors
    )
    {
        GlobalPosition = position;
        Velocity = velocity;
        LifeTime = lifetime;
        Damage = damage;
        Layer = ColLayer;
        PoolKey = poolKey;
        Priority = priority;

        Behaviors.Clear();
        Behaviors.AddRange(behaviors);

        Rotation = Velocity.Angle();
        Visible = true;
        Active = true;
    }
    public void Deactivate()
    {
        Visible = false;
        Active = false;
    }

    public void AddDisplacement(Vector2 deltaMove)
    {
        PendingDisplacement += deltaMove;
    }

    private void MoveWithGridRay(Vector2 displacement)
    {
        float maxDist = displacement.Length();
        if (maxDist <= 0f)
            return;

        Vector2 dir = displacement / maxDist;
        Vector2 pos = GlobalPosition;

        const float tileSize = 32f;
        const int MAX_STEPS = 32; // safety cap   might remove later

        float remaining = maxDist;

        for (int step = 0; step < MAX_STEPS && remaining > 0f; step++)
        {
            Vector2I cell = Main.Instance.WorldToCell(pos);

            int stepX = dir.X > 0 ? 1 : -1;
            int stepY = dir.Y > 0 ? 1 : -1;

            float nextX = (cell.X + (stepX > 0 ? 1 : 0)) * tileSize;
            float nextY = (cell.Y + (stepY > 0 ? 1 : 0)) * tileSize;

            float distX = dir.X != 0f
                ? (nextX - pos.X) / dir.X
                : float.PositiveInfinity;

            float distY = dir.Y != 0f
                ? (nextY - pos.Y) / dir.Y
                : float.PositiveInfinity;

            // Ensure positive distances
            if (distX < 0f) distX = float.PositiveInfinity;
            if (distY < 0f) distY = float.PositiveInfinity;

            float travel = Mathf.Min(distX, distY);

            // Guarantee forward progress
            if (travel <= 0f)
                travel = 0.0001f;

            if (travel > remaining)
            {
                pos += dir * remaining;
                break;
            }

            pos += dir * travel;
            remaining -= travel;

            bool hitX = distX < distY;

            Vector2I nextCell = cell + new Vector2I(
                hitX ? stepX : 0,
                hitX ? 0 : stepY
            );

            if (Main.Instance.IsWallCell(nextCell))
            {
                Vector2 normal = hitX
                    ? new Vector2(-stepX, 0)
                    : new Vector2(0, -stepY);

                GlobalPosition = pos;
                NotifyWallHit(normal);
                return;
            }

            // Nudge into the next cell to avoid precision issues
            pos += hitX
                ? new Vector2(stepX * 0.001f, 0f)
                : new Vector2(0f, stepY * 0.001f);
        }

        GlobalPosition = pos;
    }


}
