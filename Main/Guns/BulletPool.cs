using Godot;
using System.Collections.Generic;

public partial class BulletPool : Node2D
{
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
            if (pool.Count > 0 && pool.Peek().SceneFilePath != gunData.BulletScene.ResourcePath)
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

        var amount = CalculatePoolSize(gunData.BulletLifeTime, gunData.FireRate, gunData.MaxAmmo);
        //var amount = gunData.MaxAmmo;
        GD.Print("Added bullets " + amount);
        TotalBulletAmount += amount;
        GD.Print("total amount of bullets " + TotalBulletAmount);

        for (int i = pool.Count; i < amount; i++)
        {
            var bullet = gunData.BulletScene.Instantiate<Bullet>();
            //bullet.Init(gunData, this);
            CallDeferred("add_child", bullet);
            pool.Enqueue(bullet);

            if (gunData.isEnemy) _enemyBullets.Add(bullet);
        }
    }

    private int CalculatePoolSize(float lifetime, float firerate, int maxammo)
    {
        int theoretical = Mathf.CeilToInt(lifetime/ Mathf.Max(firerate, 0.0001f));
        return Mathf.Min(maxammo, Mathf.CeilToInt(theoretical * 1.1f));
    }


    public Bullet GetBullet(string key, GunData gunData)
    {
        if (!_pools.TryGetValue(key, out var pool) || pool.Count == 0)
        {
            GD.PrintErr("Ran out of Bullets to use");
            //PreparePool(key, gunData);
        }

        var bullet = _pools[key].Dequeue();
        //bullet.Visible = true;
        //UniversalStopButton.EnableNode(bullet);
        return bullet;
    }
    public void ReturnBullet(string key, Bullet bullet)
    {
        //bullet.Visible = false;
        //UniversalStopButton.DisableNode(bullet);
        _pools[key].Enqueue(bullet);
    }
    public void NewBullets(string key, GunData gunData, int amount)
    {
        if (_pools.ContainsKey(key))
        {
            foreach (var bullet in _pools[key])
            {
                if (_enemyBullets.Contains(bullet)) _enemyBullets.Remove(bullet);
                bullet.QueueFree();
            }
                

            _pools.Remove(key);
        }

        PreparePool(key, gunData);
    }
    public void ClearBullets()
    {
        //EventBus.TriggerScreenShake(0.4f);

        // foreach (var bullet in _enemyBullets)
        // {
        //     if (IsInstanceValid(bullet) && bullet.active)
        //         bullet.Deactivate();
        // }
    }
}
