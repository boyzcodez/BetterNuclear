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

        for (int i = 0; i < bulletAmount; i++)
        {
            var bullet = BulletScene.Instantiate<Bullet>();

            bullet.Behaviors.Clear();
            bullet.Init(initData);

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
        {
            GD.Print("No bullets to use " + key);
            return null;
        }
            

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
        if (lifetime <= 0f || fireRate <= 0f || bulletCount <= 0)
            return 0;

        int concurrentShots = Mathf.FloorToInt(lifetime / fireRate) + 1;
        return concurrentShots * bulletCount;
    }
}
