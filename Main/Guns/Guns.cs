using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Guns : Node2D
{
    [Export] public GunData[] guns { get; set; } = [];
    [Export] public bool active = false;
    public GunAnimation sprite { get; set; }
    private RandomNumberGenerator rng = new();

    private Dictionary<string, AudioStreamRandomizer> AudioLibrary = new ();
    private AudioStreamPlayer audioSystem;

    private BulletPool pool;
    private GunData currentGun;
    private AnimatedSprite2D muzzleFlash;
    private ShaderMaterial shaderMaterial;

    public bool shooting = false;
    private int _currentGunIndex = 0;
    private float _cooldown = 0f;
    public string id;
    
    public override void _Ready()
    {
        pool = GetTree().GetFirstNodeInGroup("BulletPool") as BulletPool;
        sprite = GetNode<GunAnimation>("GunAnimation");
        muzzleFlash = GetNode<AnimatedSprite2D>("MuzzleFlash");
        shaderMaterial = sprite.Material as ShaderMaterial;
        audioSystem = GetNode<AudioStreamPlayer>("GunAudio");

        rng.Randomize();

        //EventBus.Reset += ReFillGuns;

        foreach (var gunData in guns)
        {
            gunData.ShootAnimation.Name = gunData.GunId + "OnShoot";
            gunData.HitAnimation.Name = gunData.GunId + "OnHit";

            var bulletAmount = CalculateNeededBullets(
                gunData.BulletLifeTime,
                gunData.FireRate,
                gunData.MaxAmmo,
                gunData.BulletCount
            );

            var initData = new IBulletInitData(
                    new DamageData(
                        gunData.Damage,
                        gunData.Knockback,
                        gunData.GunId,
                        gunData.DamageType
                    ),
                    gunData.Behaviors,
                    gunData.ShootAnimation,
                    gunData.HitAnimation,
                    gunData.BulletRaidus,
                    gunData.BulletSpeed,
                    gunData.BulletLifeTime,
                    gunData.CollisionLayer,
                    gunData.GunId
                );

            pool?.PreparePool(initData, (int)(gunData.MaxAmmo * gunData.BulletCount * gunData.FireRate * gunData.BulletLifeTime));
            if (gunData.NeedsCopies)
            {
                initData.key = gunData.GunId + "Copy";
                initData.Behaviors = gunData.CopyBehaviors.Length > 0 ? gunData.CopyBehaviors : [new Normal()];
                pool?.PreparePool(initData, (int)(gunData.MaxAmmo * gunData.BulletCount * gunData.FireRate * gunData.BulletLifeTime));
            }
            
            //if (gunData.Sound != null) AudioLibrary.Add(gunData.GunName, gunData.Sound);

            if (gunData.UsesAnimations)
            {
                AddAnimation(gunData.NormalAnimationData, gunData.GunId);
                AddAnimation(gunData.ShootAnimationData, gunData.GunId + "Shoot");
            }
        }

        EquipGun(0);
        sprite?.Play(currentGun.GunId);

        // add to exp list so that when an enemy dies the correct gun will the the exp
        //if (!guns[_currentGunIndex].isEnemy) XpHandler.AddGun(guns[_currentGunIndex].GunName, this);
    }
    private static int CalculateNeededBullets(float lifetime, float fireRate, int maxAmmo, int bulletCount)
    {
        if (lifetime <= 0f || fireRate <= 0f || maxAmmo <= 0 || bulletCount <= 0)
            return 0;

        float shotsAlive = Mathf.Min(lifetime / fireRate, maxAmmo);
        return Mathf.CeilToInt(shotsAlive * bulletCount * 1.2f);
    }

    public void SwitchGuns(int direction)
    {
        _currentGunIndex = (_currentGunIndex + direction) % guns.Length;
        if (_currentGunIndex < 0) _currentGunIndex = guns.Length - 1;

        EquipGun(_currentGunIndex);
    }
    public void EquipGun(int index)
    {
        currentGun = guns[index];
        id = currentGun.GunId;

        sprite?.Play(currentGun.GunId);
        muzzleFlash.Position = currentGun.ShootPosition;
        Position = currentGun.GunSpot;
        if (AudioLibrary.ContainsKey(currentGun.GunId)) audioSystem.Stream = AudioLibrary[currentGun.GunId];
    }

    public override void _Process(double delta)
    {
        if (shooting && _cooldown <= 0) Shoot();
        else if (_cooldown > 0) _cooldown -= (float)delta;

        if (GlobalRotation > -1.5f && GlobalRotation < 1.5f)
        {
            shaderMaterial.SetShaderParameter("flip_v", false);
            muzzleFlash.Position = new Vector2(currentGun.ShootPosition.X, currentGun.ShootPosition.Y);
            Position = currentGun.GunSpot;
        }
        else
        {
            shaderMaterial.SetShaderParameter("flip_v", true);
            muzzleFlash.Position = new Vector2(currentGun.ShootPosition.X, -currentGun.ShootPosition.Y);
            Position = new Vector2(currentGun.GunSpot.X, -currentGun.GunSpot.Y);
        } 
    }

    public void Shoot()
    {
        if (currentGun == null || currentGun.CurrentAmmo <= 0)
            return;

        currentGun.UseBullet();

        Eventbus.TriggerScreenShake(
            currentGun.ShakeIntensity,
            currentGun.ShakeDuration
        );

        sprite.FireAnimation();
        PlayAnimation();

        // if (hasShootSound)
        //     audioSystem.Play();

        Vector2 muzzlePos = muzzleFlash.GlobalPosition;

        float baseAngle = GlobalRotation;
        float halfSpreadRad = Mathf.DegToRad(currentGun.SpreadAngle * 0.5f);

        for (int i = 0; i < currentGun.BulletCount; i++)
        {
            float angle = baseAngle + rng.RandfRange(-halfSpreadRad, halfSpreadRad);

            Bullet bullet = pool.GetBullet(id);
            if (bullet == null)
                break;

            bullet.GlobalPosition = muzzlePos;
            bullet.Rotation = angle;
            bullet.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();

            bullet.Activate();
        }

        _cooldown = currentGun.FireRate;
    }
    private async void PlayAnimation()
    {
        muzzleFlash.Play("default");
        sprite.Play(currentGun.GunId + "Shoot");

        await ToSignal(sprite, "animation_finished");

        sprite.Play(currentGun.GunId);
    }
    private float NumBet(double bet)
    {
        return (float)GD.RandRange(-bet, bet);
    }
    private void ReFillGuns()
    {
        foreach (GunData gun in guns)
        {
            gun.ReFillAmmo(999);
        }
    }

    public void AddAnimation(AnimationData AnimationData, string name)
    {
        if (sprite.SpriteFrames.HasAnimation(name)) return;

        sprite.SpriteFrames.AddAnimation(name);

        int totalFrames = AnimationData.HorizontalFrames * AnimationData.VerticalFrames;

        for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
        {
            int x = frameIndex % AnimationData.HorizontalFrames;
            int y = frameIndex / AnimationData.HorizontalFrames;

            var region = new Rect2I(
                x * (AnimationData.SpriteSheet.GetWidth() / AnimationData.HorizontalFrames),
                y * (AnimationData.SpriteSheet.GetHeight() / AnimationData.VerticalFrames),
                AnimationData.SpriteSheet.GetWidth() / AnimationData.HorizontalFrames,
                AnimationData.SpriteSheet.GetHeight() / AnimationData.VerticalFrames
            );

            var frameTexture = AnimationData.SpriteSheet.GetImage().GetRegion(region);
            var tex = ImageTexture.CreateFromImage(frameTexture);

            sprite.SpriteFrames.AddFrame(name, tex);
        }
        
        sprite.SpriteFrames.SetAnimationSpeed(name, AnimationData.FrameRate);
        sprite.SpriteFrames.SetAnimationLoop(name, AnimationData.Looping);

        sprite.Play(name);
    }
}
