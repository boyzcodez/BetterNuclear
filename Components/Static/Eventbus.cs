using Godot;
using System;

public static class Eventbus
{
    // Enemy stuff
    public static event Action<TileMapLayer> SpawnEnemies;
    public static event Action EnemiesKilled;

    // Effects and stuff
    public static event Action<float, float> ScreenShake;
    public static event Action<string, Vector2> SpawnItem;
    public static event Action<int, Vector2> Explosion;

    // Game stuff
    public static event Action Reset;


    // Stats tracking
    public static int dangerValue = 0;
    public static bool gameOn = false;


    // enemy stuff
    public static void TriggerSpawnEnemies(TileMapLayer map) =>
        SpawnEnemies?.Invoke(map);
    public static void TriggerEnemiesKilled() =>
        EnemiesKilled?.Invoke();


    // effect and stuff
    public static void TriggerScreenShake(float intensity, float duration) =>
        ScreenShake?.Invoke(intensity, duration);
    public static void TriggerSpawnItem(string item, Vector2 position) =>
        SpawnItem?.Invoke(item, position);
    public static void TriggerExplosion(int size, Vector2 position) =>
        Explosion?.Invoke(size, position);
    
    // Game stuff
    public static void TriggerReset() =>
        Reset?.Invoke();
}
