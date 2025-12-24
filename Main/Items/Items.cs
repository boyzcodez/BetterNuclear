using Godot;
using System.Collections.Generic;

public partial class Items : Node2D
{
    [Export] public ItemResource[] itemScenes = [];
    public Dictionary<string, Queue<Bullet>> _pools = new();
    private int TotalBulletAmount = 0;

    public override void _Ready()
    {
        foreach (var item in itemScenes)
        {
            PreparePool(item);
        }
    }

    public void PreparePool(ItemResource item)
    {
        if (_pools.TryGetValue(item.Name, out var pool))
        {
            if (pool.Count > 0)
            {
                foreach (var poolItem in pool)
                {
                    poolItem.QueueFree();
                }
                pool.Clear();
            }
        }
        else
        {
            pool = new Queue<Bullet>();
            _pools[item.Name] = pool;
        }

        for (int i = pool.Count; i < item.AmountOfItem; i++)
        {
            var instance = item.itemScene.Instantiate();
            //CallDeferred("add_child", instance);
            AddChild(instance);
        }

    }
}
