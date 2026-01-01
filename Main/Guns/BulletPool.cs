using Godot;
using System.Collections.Generic;

public partial class BulletPool : Node2D
{
    [Export] public PackedScene BulletScene;

    private sealed class Pool
    {
        public readonly Stack<Bullet> Free = new();
        public int CreatedCount = 0;
    }

    private readonly Dictionary<StringName, Pool> _pools = new();
    private int _totalCreated = 0;

    public void PreparePool(IBulletInitData initData, int bulletAmount)
    {
        var key = initData.key;

        if (!_pools.TryGetValue(key, out var pool))
        {
            pool = new Pool();
            _pools[key] = pool;
        }

        // int target = CalculatePoolSize(
        //     gunData.BulletLifeTime,
        //     gunData.FireRate,
        //     gunData.MaxAmmo,
        //     gunData.BulletCount
        // );

        // Only grow (never shrink)
        //int toCreate = target - pool.CreatedCount;
        //if (toCreate <= 0) return;

        for (int i = 0; i < bulletAmount; i++)
        {
            var bullet = BulletScene.Instantiate<Bullet>();
            bullet.Init(initData);

            bullet.Behaviors.Clear();
            foreach (var beh in initData.Behaviors)
                bullet.Behaviors.Add(beh.CreateBehavior());

            AddChild(bullet);

            // IMPORTANT: pooled bullets should not be "doing stuff"

            pool.Free.Push(bullet);
            pool.CreatedCount++;
            _totalCreated++;
        }
    }

    public Bullet GetBullet(StringName key)
    {
        if (!_pools.TryGetValue(key, out var pool) || pool.Free.Count == 0)
            return null;

        var bullet = pool.Free.Pop();

        Eventbus.activeBullets += 1;
        return bullet;
    }

    public void ReturnBullet(StringName key, Bullet bullet)
    {
        if (!IsInstanceValid(bullet)) return;

        if (_pools.TryGetValue(key, out var pool))
            pool.Free.Push(bullet);
        // else: unknown key, ignore or push into a "misc" pool

        Eventbus.activeBullets -= 1;
    }

    private static int CalculatePoolSize(float lifetime, float fireRate, int maxAmmo, int bulletCount)
    {
        if (lifetime <= 0f || fireRate <= 0f || maxAmmo <= 0 || bulletCount <= 0)
            return 0;

        float shotsAlive = Mathf.Min(lifetime / fireRate, maxAmmo);
        return Mathf.CeilToInt(shotsAlive * bulletCount);
    }
}
