using Godot;
using System;

public class IBulletInitData
{
    public DamageData damageData;
    public Animation animationData;
    public BulletPool pool;
    public float BulletRadius {get; set;}
    public float BulletSpeed { get; set; }
    public float BulletLifeTime {get;set;}
    public int CollisionLayer {get;set;}
    public string key {get; set;}

    public IBulletInitData(DamageData newDMG, Animation newANIM, float newRadius, float newSpeed, float newLifeTime, int newLayer, string newKey, BulletPool newPool)
    {
        damageData = newDMG;
        animationData = newANIM;
        BulletRadius = newRadius;
        BulletSpeed = newSpeed;
        BulletLifeTime = newLifeTime;
        CollisionLayer = newLayer;
        key = newKey;
        pool = newPool;
    }
}
