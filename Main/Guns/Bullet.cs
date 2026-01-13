using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Bullet : AnimatedSprite2D, ICollidable
{
    public Vector2 Velocity;
    public float Radius = 5f;
    public float LifeTime = 5f;
    public float _lifeTime = 5f;
    public int CollisionLayer = 2; // 2 PlayerBullet, 1 EnemyBullet, etc
    // this is a test comment for a new branch

    public Vector2 _Position => GlobalPosition;
    float ICollidable.CollisionRadius => Radius;
    int ICollidable.CollisionLayer => CollisionLayer;

    public StringName key;
    public DamageData damageData;
    public List<IBulletBehavior> Behaviors = new();

    public bool Active = false;
    public bool HasHit { get; private set; }
    public Vector2 PendingDisplacement { get; private set; }

    public Main main;
    public BulletPool pool;

    public float Speed = 200f;
    private string OnShoot;
    private string OnHit;

    public override void _Ready()
    {
        pool = GetTree().GetFirstNodeInGroup("BulletPool") as BulletPool;
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        main.bullets.Add(this);

        Visible = false;
        AnimationFinished += OnAnimationEnd;
    }

    public void Update(double delta)
    {
        Vector2 displacement = PendingDisplacement;
        PendingDisplacement = Vector2.Zero;

        MoveWithGridRay(displacement);

        foreach (var behavior in Behaviors) behavior.OnUpdate(this, (float)delta);

        LifeTime -= (float)delta;

        if (LifeTime <= 0f) Deactivate();
    }

    private void NotifyWallHit(Vector2 normal)
    {
        foreach (var b in Behaviors) b.OnWallHit(this, normal);
    }

    public void Activate()
    {
        foreach (var b in Behaviors) b.OnSpawn(this);

        Play(OnShoot);

        LifeTime = _lifeTime;

        Active = true;
        Visible = true;
    }
    public void Deactivate()
    {
        Play(OnHit);

        Velocity = Vector2.Zero;
        Active = false;
    }
    public void OnAnimationEnd()
    {
        Visible = false;
        pool.ReturnBullet(key, this);
    }

    public void Init(IBulletInitData data)
    {
        damageData = data.damageData;
        _lifeTime = data.BulletLifeTime;
        CollisionLayer = data.CollisionLayer;
        Radius = data.BulletRadius;
        Speed = data.BulletSpeed;
        key = data.key;

        OnShoot = data.ShootAnimation.Name;
        OnHit = data.HitAnimation.Name;

        foreach (var behavior in data.Behaviors)
        {
            var instance = (IBulletBehavior)behavior.Duplicate(true);
            Behaviors.Add(instance);
        }

        if (SpriteFrames.HasAnimation(OnShoot)) return;

        AnimatedSpriteBuilder.BuildAnimation(this, data.ShootAnimation);
        AnimatedSpriteBuilder.BuildAnimation(this, data.HitAnimation);
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
