using Godot;
using System;

public class IBulletInitData
{
    public DamageData damageData;
    public Animation ShootAnimation;
    public Animation HitAnimation;
    public BulletPool pool;
    public float BulletRadius {get; set;}
    public float BulletSpeed { get; set; }
    public float BulletLifeTime {get;set;}
    public int CollisionLayer {get;set;}
    public StringName key {get; set;}

    public IBulletInitData(DamageData newDMG, Animation newShoot, Animation newHit, float newRadius, float newSpeed, float newLifeTime, int newLayer, StringName newKey, BulletPool newPool)
    {
        damageData = newDMG;
        ShootAnimation = newShoot;
        HitAnimation = newHit;
        BulletRadius = newRadius;
        BulletSpeed = newSpeed;
        BulletLifeTime = newLifeTime;
        CollisionLayer = newLayer;
        key = newKey;
        pool = newPool;
    }
}
