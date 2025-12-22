using Godot;
using System;

public class IBulletInitData
{
    public DamageData damageData;
    public Animation animationData;
    public BulletPool pool;
    public float BulletSpeed { get; set; }
    public float BulletLifeTime {get;set;}
    public int CollisionLayer {get;set;}
    public string key {get; set;}

    public IBulletInitData(DamageData newDMG, Animation newANIM, float newSpeed, float newLifeTime, int newLayer, string newKey, BulletPool newPool)
    {
        damageData = newDMG;
        animationData = newANIM;
        BulletSpeed = newSpeed;
        BulletLifeTime = newLifeTime;
        CollisionLayer = newLayer;
        key = newKey;
        pool = newPool;
    }
}
