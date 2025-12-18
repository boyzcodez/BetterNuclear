using Godot;
using System;

public static class Eventbus
{
    public static event Action<TileMapLayer> SpawnEnemies;



    public static int dangerValue = 0;
    public static bool gameOn = false;


    public static void TriggerSpawnEnemies(TileMapLayer map) =>
        SpawnEnemies?.Invoke(map);
}
