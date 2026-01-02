using Godot;
using System;

[GlobalClass]
public partial class GunData : Resource
{
    [Export] public StringName GunId {get; set; } = "";
    [Export(PropertyHint.Enum, "LV1_,LV2_,LV3_,LV4_,LV5_")]
    public string LVL { get; set; } = "LV1_";
    
    [Export(PropertyHint.Enum, "Impact,Obliterate,Disintegrate")]
    public string DamageType { get; set; } = "Impact";
    
    [Export] public bool UsesAmmo { get; set; } = true;
    [Export] public int CurrentAmmo { get; set; } = 10;
    [Export] public int MaxAmmo { get; set; } = 10;
    [Export] public float FireRate { get; set; } = 0.2f;
    [Export] public int BulletCount { get; set; } = 1;
    [Export] public float SpreadAngle { get; set; } = 0f;
    [Export] public float RandomFactor { get; set; } = 0f;
    

    [ExportGroup("XP")]
    [Export] public int currentXP { get; set; } = 0;
    [Export] public int maxXP { get; set; } = 10;
    [Export] public GunData NextLevelData { get; set; }

    [ExportGroup("Bullet")]
    [Export] public Animation ShootAnimation {get; set;}
    [Export] public Animation HitAnimation {get; set;}
    
    [Export] public Vector2 ShootPosition { get; set; }
    [Export] public Vector2 GunSpot { get; set; } = new Vector2 (6, 0);
    [Export] public Texture2D icon { get; set; }

    [Export] public int CollisionLayer {get; set;} = 2; //1 Player, 2 Enemy, 3 ???
    [Export] public int Damage { get; set; } = 1;
    [Export] public float Knockback { get; set; } = 0f;
    [Export] public float BulletRaidus {get; set;} = 5f;
    [Export] public float BulletSpeed { get; set; } = 140f;
    [Export] public float BulletLifeTime {get;set;} = 4f;
    [Export] public BehaviorResource[] Behaviors {get;set;} = [];
    [Export] public bool NeedsCopies {get;set;} = false;
    [Export] public BehaviorResource[] CopyBehaviors {get;set;} = [];

    [ExportGroup("Animations")]
    [Export] public AnimationData NormalAnimationData {get;set;}
    [Export] public AnimationData ShootAnimationData {get;set;}
    [Export] public bool UsesAnimations {get;set;} = true;
    [Export] public float ShakeIntensity {get; set;} = 0f;
    [Export] public float ShakeDuration {get; set;} = 0f;
    
    // [ExportGroup("Gun Parts")]
    // [Export] public bool isEnemy { get; set; } = false;
    // [Export] public bool rotate { get; set; } = false;
    // [Export] public bool LaserSight { get; set; } = false;
    // [Export] public AudioStreamRandomizer Sound {get;set;}
    

    public void UseBullet()
    {
        if (UsesAmmo) CurrentAmmo -= 1;
    }
    public void ReFillAmmo(int ammoAmount)
    {
        CurrentAmmo += ammoAmount;
        CurrentAmmo = Math.Clamp(CurrentAmmo, 0, MaxAmmo);
    }
}
