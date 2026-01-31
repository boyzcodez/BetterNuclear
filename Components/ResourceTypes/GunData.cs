using Godot;
using System;

public enum DamageTypes
{
    Impact,
    Obliterate,
    Disintegrate,
    Unkown
}
public enum BulletSpawnPoint
{
    Muzzle,
    Mouse
}

[GlobalClass]
public partial class GunData : Resource
{
    [Export] public StringName GunId {get; set; } = "";
    [Export] public DamageTypes DamageType { get; set; } = DamageTypes.Impact;
    [Export] public Texture2D Icon {get; set;}
    
    [Export] public bool UsesAmmo { get; set; } = true;
    [Export] public int CurrentAmmo { get; set; } = 10;
    [Export] public int MaxAmmo { get; set; } = 10;
    [Export] public float FireRate { get; set; } = 0.2f;
    [Export] public int BulletCount { get; set; } = 1;
    [Export] public float SpreadAngle { get; set; } = 0f;
    [Export] public float RandomFactor { get; set; } = 0f;

    [ExportGroup("Bullet")]
    
    // [Export] public Animation ShootAnimation {get; set;}
    // [Export] public Animation HitAnimation {get; set;}
    [Export] public BulletSpawnPoint SpawnPoint { get; set; } = BulletSpawnPoint.Muzzle;
    [Export] public Vector2 ShootPosition { get; set; }
    [Export] public Vector2 GunSpot { get; set; } = new Vector2 (6, 0);

    [Export] public int Bounces {get; set;} = 0;
    [Export] public int Pierces {get; set;} = 0;

    [Export] public int CollisionLayer {get; set;} = 2; //1 Player, 2 Enemy, 3 ???
    [Export] public float Damage { get; set; } = 1f;
    [Export] public float Knockback { get; set; } = 0f;
    [Export] public float BulletRaidus {get; set;} = 5f;
    [Export] public Shape2D Shape { get; set;}
    [Export] public float BulletSpeed { get; set; } = 140f;
    [Export] public float BulletLifeTime {get;set;} = 4f;
    [Export] public BulletPriority priority = BulletPriority.Normal;
    [Export] public BehaviorResource[] Behaviors {get;set;} = [];
    [Export] public BehaviorResource[] OriginalBehavior {get; set;} = [];
    public IBulletData BulletData {get; set;}

    [ExportGroup("Animations ETC")]
    
    [Export] public AnimationData NormalAnimationData {get;set;}
    [Export] public AnimationData ShootAnimationData {get;set;}
    [Export] public AnimationData ChargeAnimationData {get; set;}
    [Export] public bool UsesAnimations {get;set;} = true;
    [Export] public float ShakeIntensity {get; set;} = 0f;
    [Export] public float ShakeDuration {get; set;} = 0f;
    [Export] public bool AlwaysBehindParent {get; set;} = false;
    [Export] public bool DoesntRotate {get; set;} = false;

    

    public void UseBullet()
    {
        if (UsesAmmo) CurrentAmmo -= 1;
    }
    public void ReFillAmmo(float ammoPer = 0.3f)
    {
        ammoPer = Math.Clamp(ammoPer, 0f,1f);
        CurrentAmmo = (int)(MaxAmmo * ammoPer);

        CurrentAmmo = Math.Clamp(CurrentAmmo, 0, MaxAmmo);
    }
}
