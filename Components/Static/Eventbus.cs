using Godot;
using System;

public static class Eventbus
{
    public static event Action<TileMapLayer> SpawnEnemies;
    public static event Action EnemiesKilled;

    public static int dangerValue = 0;
    public static bool gameOn = false;


    public static void TriggerSpawnEnemies(TileMapLayer map) =>
        SpawnEnemies?.Invoke(map);
    public static void TriggerEnemiesKilled() =>
        EnemiesKilled?.Invoke();
}
