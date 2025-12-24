using Godot;
using System.Collections.Generic;

public partial class BulletPool : Node2D
{
    [Export] public PackedScene bulletScene;
    public Dictionary<string, Queue<Bullet>> _pools = new();
    public List<Bullet> _enemyBullets = new();
    private int TotalBulletAmount = 0;

    public override void _Ready()
    {
        // EventBus.ClearBullets += ClearBullets;
        // EventBus.Reset += ClearBullets;
    }

    public void PreparePool(string key, GunData gunData)
    {
        if (_pools.TryGetValue(key, out var pool))
        {
            if (pool.Count > 0)
            {
                foreach (var bullet in pool)
                {
                    bullet.QueueFree();
                    if (_enemyBullets.Contains(bullet)) _enemyBullets.Remove(bullet);
                }
                pool.Clear();
            }
        }
        else
        {
            pool = new Queue<Bullet>();
            _pools[key] = pool;
        }

        var amount = CalculatePoolSize(gunData.BulletLifeTime, gunData.FireRate, gunData.MaxAmmo, gunData.BulletCount);
        TotalBulletAmount += amount;
        //GD.Print("total amount of bullets " + TotalBulletAmount);

        for (int i = pool.Count; i < amount; i++)
        {
            var bullet = bulletScene.Instantiate<Bullet>();

            // making a data set for the bullet
            bullet.Init(
                new IBulletInitData(
                    new DamageData(
                        gunData.Damage,
                        gunData.Knockback,
                        gunData.Name,
                        gunData.DamageType
                    ),
                    gunData.ShootAnimation,
                    gunData.HitAnimation,
                    gunData.BulletRaidus,
                    gunData.BulletSpeed,
                    gunData.BulletLifeTime,
                    gunData.CollisionLayer,
                    key,
                    this
                )
            );

            // giving the bullet it's behaviors
            foreach (var beh in gunData.Behaviors)
            {
                bullet.Behaviors.Add(beh.CreateBehavior());
            }

            //CallDeferred("add_child", bullet);
            AddChild(bullet);
            pool.Enqueue(bullet);

            //if (gunData.isEnemy) _enemyBullets.Add(bullet);
        }
    }

    private int CalculatePoolSize(float lifetime, float firerate, int maxammo, int bulletCount)
    {
        // int theoretical = Mathf.CeilToInt(lifetime / Mathf.Max(firerate, 0.0001f));
        // return Mathf.Min(maxammo, Mathf.CeilToInt(theoretical * 1.2f * bulletCount));

        if (lifetime <= 0f || firerate <= 0f || bulletCount <= 0)
        return 0;

        float firingDuration = maxammo * firerate;

        // How long bullets can overlap
        float overlapTime = Mathf.Min(lifetime, firingDuration);

        // Number of shots during the overlap
        float shotsDuringOverlap = overlapTime / firerate;

        // Total bullets alive at once
        float simultaneousBullets = shotsDuringOverlap * bulletCount;

        return Mathf.CeilToInt(simultaneousBullets);
    }


    public Bullet GetBullet(string key)
    {
        if (!_pools.TryGetValue(key, out var pool) || pool.Count == 0)
        {
            GD.PrintErr("Ran out of Bullets to use");
            //PreparePool(key, gunData);
            return null;
        }

        var bullet = _pools[key].Dequeue();
        return bullet;
    }
    public void ReturnBullet(string key, Bullet bullet)
    {
        _pools[key].Enqueue(bullet);
    }

    // public void NewBullets(string key, GunData gunData, int amount)
    // {
    //     if (_pools.ContainsKey(key))
    //     {
    //         foreach (var bullet in _pools[key])
    //         {
    //             if (_enemyBullets.Contains(bullet)) _enemyBullets.Remove(bullet);
    //             bullet.QueueFree();
    //         }
                

    //         _pools.Remove(key);
    //     }

    //     PreparePool(key, gunData);
    // }
    // public void ClearBullets()
    // {
    //     EventBus.TriggerScreenShake(0.4f);

    //     foreach (var bullet in _enemyBullets)
    //     {
    //         if (IsInstanceValid(bullet) && bullet.active)
    //             bullet.Deactivate();
    //     }
    // }
}
