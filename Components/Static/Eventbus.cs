using Godot;
using System;

public static class Eventbus
{
    // enemy stuff
    public static event Action<TileMapLayer> SpawnEnemies;
    public static event Action EnemiesKilled;

    // effects and stuff
    public static event Action<float, float> ScreenShake;


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
}
