using Godot;
using System.Collections.Generic;

public partial class Items : Node2D
{
    [Export] private ItemResource[] items = [];
    public Dictionary<string, Queue<Node2D>> _pools = new();
    private int TotalBulletAmount = 0;

    public override void _Ready()
    {
        Eventbus.SpawnItem += SpawnItem;

        foreach (var item in items)
        {
            PreparePool(item);
        }
    }

    public void PreparePool(ItemResource item)
    {
        if (_pools.TryGetValue(item.Id, out var pool))
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
            pool = new Queue<Node2D>();
            _pools[item.Id] = pool;
        }

        for (int i = pool.Count; i < item.AmountOfItem; i++)
        {
            var instance = item.itemScene.Instantiate() as Node2D;
            instance.Visible = false;
            if (instance is ICollectable collectable)
            {
                collectable.Init(item.Id, this);
            }
            AddChild(instance);
            pool.Enqueue(instance);
        }

        GD.Print(item.Id + " : " + item.AmountOfItem);

    }

    public void SpawnItem(string item, Vector2 position)
    {
        if (!_pools.TryGetValue(item, out var pool) || pool.Count == 0)
        {
            GD.PrintErr("Ran out of item " + item.Capitalize() +" to use");
            return;
        }

        Node2D getItem = _pools[item].Dequeue();
        getItem.GlobalPosition = position;
        getItem.Visible = true;
        if (getItem is ICollectable collectable)
        {
            collectable.OnActivation();
        }
    }
    
    public void ReturnItem(string key, Node2D item)
    {
        item.Visible = false;
        _pools[key].Enqueue(item);
    }
}
