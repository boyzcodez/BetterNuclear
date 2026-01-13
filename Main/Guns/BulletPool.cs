using Godot;
using System.Collections.Generic;

public partial class BulletPool : Node2D
{
    public static BulletPool Instance { get; private set; }

    [Export] public PackedScene BulletScene;
    [Export] public int MaxBullets = 20000;

    private sealed class Pool
    {
        public readonly Stack<ModularBullet> Free = new();
        public readonly List<ModularBullet> Active = new();
    }

    private readonly Dictionary<StringName, Pool> _pools = new();
    private readonly List<ModularBullet> _allActive = new();

    public override void _Ready()
    {
        Instance = this;
    }

    // --- Pool prep ---
    public void PreparePool(StringName key, int amount)
    {
        if (!_pools.TryGetValue(key, out var pool))
        {
            pool = new Pool();
            _pools[key] = pool;
        }

        for (int i = 0; i < amount; i++)
        {
            var bullet = BulletScene.Instantiate<ModularBullet>();
            bullet.Visible = false;
            AddChild(bullet);
            pool.Free.Push(bullet);
        }
    }

    // --- Spawn ---
    public ModularBullet Spawn(
        StringName key,
        Vector2 position,
        Vector2 velocity,
        float lifetime,
        float damage,
        int CollisionLayer,
        BulletPriority priority,
        IEnumerable<IBulletBehavior> behaviors
    )
    {
        if (_allActive.Count >= MaxBullets)
            CullLowestPriority(priority);

        if (!_pools.TryGetValue(key, out var pool) || pool.Free.Count == 0)
        {
            GD.Print("Returned no bullet here");
            return null;
        }
            

        var bullet = pool.Free.Pop();
        pool.Active.Add(bullet);
        _allActive.Add(bullet);

        bullet.Activate(
            position,
            velocity,
            lifetime,
            damage,
            key,
            CollisionLayer,
            priority,
            behaviors
        );

        return bullet;
    }

    // --- Release ---
    public void Release(ModularBullet bullet)
    {
        if (!IsInstanceValid(bullet))
            return;

        bullet.Visible = false;

        if (_pools.TryGetValue(bullet.PoolKey, out var pool))
        {
            pool.Active.Remove(bullet);
            pool.Free.Push(bullet);
        }

        _allActive.Remove(bullet);
    }

    // --- Priority culling ---
    private void CullLowestPriority(BulletPriority incomingPriority)
    {
        for (int i = 0; i < _allActive.Count; i++)
        {
            if (_allActive[i].Priority < incomingPriority)
            {
                _allActive[i].Deactivate();
                return;
            }
        }
    }
}

public enum BulletPriority
{
    Trash,
    Normal,
    Important,
    Critical
}
