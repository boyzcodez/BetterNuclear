using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyPool : Node2D
{
    [Export] private EnemyData[] enemies = [];

    private Dictionary<string, Queue<Enemy>> pools = new();
    private List<Enemy> currentEnemies = new ();

    private RandomNumberGenerator rng = new RandomNumberGenerator();

    private List<Vector2> validSpawnPoints = new();
    private TileMapLayer groundMap;

    const int enemyAmount = 10;

    public override void _Ready()
    {
        //groundMap = GetTree().GetFirstNodeInGroup("Ground") as TileMapLayer;
        PreparePool();

        Eventbus.SpawnEnemies += CalcRound;
    }


    private void PreparePool()
    {
        foreach (var enemy in enemies)
        {
            var pool = new Queue<Enemy>();
            pools[enemy.name] = pool;

            for (int i = 0; i < enemyAmount; i++)
            {
                var Instance = enemy.enemyScene.Instantiate<Enemy>();
                Instance.name = enemy.name;
                Instance.pool = this;

                CallDeferred("add_child", Instance);
                pool.Enqueue(Instance);
            }
        }
    }



    private void CalcRound(TileMapLayer NewGeneration)
    {
        groundMap = NewGeneration;
        CalcSpawnPoints();
        ResetEnemies();

        rng.Randomize();
        int enemyCount = EnemiesPerRound();

        List<Vector2> availableSpots = validSpawnPoints;
        availableSpots = availableSpots.OrderBy(_ => rng.Randi()).ToList();

        List<EnemyData> weightedPool = BuildWeightedPool();

        for (int i = 0; i < enemyCount; i++)
        {
            int index = rng.RandiRange(0, availableSpots.Count - 1);
            Vector2 spot = availableSpots[index];
            availableSpots.RemoveAt(index);

            string chosen = weightedPool[rng.RandiRange(0, weightedPool.Count -1)].name;
            SummonEnemy(spot, chosen);
        }
    }

    private void CalcSpawnPoints()
    {
        validSpawnPoints.Clear();

        foreach (Vector2I cell in groundMap.GetUsedCells())
        {
            Vector2 worldPos = groundMap.MapToLocal(cell);

            validSpawnPoints.Add(worldPos);
        }
    }

    // i need to count the enemies for the round in this function
    private int EnemiesPerRound()
    {
        return 5;
    }

    private List<EnemyData> BuildWeightedPool()
    {
        List<EnemyData> pool = new List<EnemyData>();

        foreach (EnemyData e in enemies)
        {
            int weight = Mathf.Max(1, 10 - e.value);

            for (int i = 0; i < weight; i++) pool.Add(e);
        }

        return pool;
    }



    private void SummonEnemy(Vector2 spot, string chosen)
    {
        if (!pools.TryGetValue(chosen, out var pool) || pool.Count == 0)
        {
            GD.PrintErr("No Such Enemy");
            return;
        }

        var selected = pools[chosen].Dequeue();

        currentEnemies.Add(selected);

        selected.GlobalPosition = spot;
        selected.EmitSignal("Activation");
    }
    private void Return(Enemy enemy)
    {
        pools[enemy.name].Enqueue(enemy);
    }
    private void ResetEnemies()
    {
        foreach (var enemy in currentEnemies)
        {
            if (enemy.active) enemy.Deactivate();
            Return(enemy);
        }
        currentEnemies.Clear();
    }


}
