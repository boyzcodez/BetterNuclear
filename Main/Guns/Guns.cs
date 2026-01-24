using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Guns : Node2D
{
    [Export] public AnimatedSprite2D ChargeAnimation;
    [Export] public GunData[] guns { get; set; } = [];
    [Export] public bool active = false;

    public GunAnimation sprite { get; set; }
    private RandomNumberGenerator rng = new();

    private Dictionary<string, AudioStreamRandomizer> AudioLibrary = new();
    private AudioStreamPlayer audioSystem;

    private Look parent;
    private BulletPool pool;
    public GunData currentGun;

    private AnimatedSprite2D muzzleFlash;
    private ShaderMaterial shaderMaterial;

    public bool shooting = false;

    private int _currentGunIndex = 0;
    private float _cooldown = 0f;
    public string id;

    // Charge state
    private bool _prevShooting = false;
    private bool _isCharging = false;
    private bool _chargeReady = false;
    private int _chargeToken = 0;  // cancels old async charge if gun switches / released early

    private bool RequiresCharge(GunData g)
        => g != null && g.ChargeAnimationData != null;

    private string ChargeAnimName(GunData g)
        => $"{g.GunId}Charge";

    public override void _Ready()
    {
        pool = GetTree().GetFirstNodeInGroup("BulletPool") as BulletPool;
        sprite = GetNode<GunAnimation>("GunAnimation");
        muzzleFlash = GetNode<AnimatedSprite2D>("MuzzleFlash");
        shaderMaterial = sprite.Material as ShaderMaterial;
        audioSystem = GetNode<AudioStreamPlayer>("GunAudio");
        parent = GetParent<Look>();

        rng.Randomize();

        if (ChargeAnimation != null)
        {
            ChargeAnimation.Visible = false;
            if (ChargeAnimation.SpriteFrames != null && ChargeAnimation.SpriteFrames.HasAnimation("default"))
                ChargeAnimation.Play("default");
            else
                ChargeAnimation.Stop();
        }

        foreach (var gunData in guns)
        {
            var itemsPool = GetTree().GetFirstNodeInGroup("ItemsPool") as Items;

            foreach (var item in gunData.items)
                itemsPool.PreparePool(item);

            gunData.BulletData = new IBulletData(
                gunData.priority,
                gunData.Shape,
                gunData.Bounces,
                gunData.Pierces,
                new DamageData(gunData.Damage, gunData.Knockback, gunData.GunId, gunData.DamageType),
                gunData.Behaviors,
                gunData.BulletRaidus,
                gunData.BulletSpeed,
                gunData.BulletLifeTime,
                gunData.CollisionLayer,
                gunData.GunId
            );

            if (gunData.UsesAnimations)
            {
                AddAnimation(gunData.NormalAnimationData, gunData.GunId);
                AddAnimation(gunData.ShootAnimationData, gunData.GunId + "Shoot");

                // charge animation = charging weapon
                if (gunData.ChargeAnimationData != null)
                    AddAnimation(gunData.ChargeAnimationData, gunData.GunId + "Charge");
            }
        }

        if (guns.Length > 0)
        {
            EquipGun(0);
            sprite?.Play(currentGun.GunId);
        }
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

        // Reset charge state on gun switch
        CancelChargeState();

        sprite?.Play(currentGun.GunId);
        muzzleFlash.Position = currentGun.ShootPosition;
        Position = currentGun.GunSpot;
        parent.Lock(currentGun.DoesntRotate);

        if (AudioLibrary.ContainsKey(currentGun.GunId))
            audioSystem.Stream = AudioLibrary[currentGun.GunId];
    }

    public void SetGunBehindParent(bool bl)
    {
        if (guns.Length <= 0) return;

        if (currentGun.AlwaysBehindParent) parent?.ShowBehindParent = true;
        else parent?.ShowBehindParent = bl;
    }

    public override void _Process(double delta)
    {
        if (currentGun == null) return;

        // cooldown timer
        if (_cooldown > 0f) _cooldown -= (float)delta;

        // detect press/release edges from your existing `shooting` bool
        bool justPressed = shooting && !_prevShooting;
        bool justReleased = !shooting && _prevShooting;
        _prevShooting = shooting;

        if (RequiresCharge(currentGun))
        {
            if (justPressed)
                TryStartCharge();

            if (justReleased)
            {
                // Release fires only if charge complete, otherwise cancels charge
                if (_chargeReady)
                {
                    HideChargeIndicator();
                    _chargeReady = false;

                    if (_cooldown <= 0f)
                        Shoot();
                }
                else if (_isCharging)
                {
                    // released before ready: cancel
                    CancelChargeState();
                }
            }
        }
        else
        {
            // Normal weapon flow
            if (shooting && _cooldown <= 0f)
                Shoot();
        }

        // flip/position logic (unchanged)
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

    private void TryStartCharge()
    {
        if (currentGun == null) return;
        if (_cooldown > 0f) return;
        if (currentGun.CurrentAmmo <= 0) return;

        if (Main.Instance.IsWallAt(muzzleFlash.GlobalPosition)) return;

        if (_isCharging || _chargeReady) return;

        HideChargeIndicator();
        _isCharging = true;
        _chargeReady = false;

        _chargeToken++;
        int token = _chargeToken;

        // Ensure normal playback speed for the charge anim
        if (sprite != null) sprite.SpeedScale = 1f;

        StartChargeAsync(token);
    }

    private async void StartChargeAsync(int token)
    {
        string animName = ChargeAnimName(currentGun);

        // If there's no charge anim, treat as instantly ready
        if (sprite == null || sprite.SpriteFrames == null || !sprite.SpriteFrames.HasAnimation(animName))
        {
            if (token != _chargeToken) return;
            if (!shooting) { _isCharging = false; return; }

            _isCharging = false;
            SetChargeReadyHoldLastFrame(animName);
            return;
        }

        sprite.Play(animName);

        await ToSignal(sprite, "animation_finished");

        // canceled or switched guns?
        if (token != _chargeToken) return;

        // player must still be holding to become "ready"
        if (!shooting)
        {
            _isCharging = false;
            return;
        }

        _isCharging = false;
        SetChargeReadyHoldLastFrame(animName);
    }

    // IMPORTANT: this holds the gun on the final charge frame using PAUSE (not Stop)
    private void SetChargeReadyHoldLastFrame(string animName)
    {
        _chargeReady = true;

        // indicator
        if (ChargeAnimation != null)
        {
            ChargeAnimation.Visible = true;
            ChargeAnimation.Play("default");
        }

        if (sprite == null || sprite.SpriteFrames == null) return;
        if (!sprite.SpriteFrames.HasAnimation(animName)) return;

        int last = sprite.SpriteFrames.GetFrameCount(animName) - 1;
        if (last < 0) return;

        // Make sure we're on the charge animation, then force last frame, then PAUSE to keep it
        sprite.Animation = animName;
        sprite.Frame = last;
        sprite.FrameProgress = 0f;

        // Pause keeps the current frame. Stop() would reset to frame 0.
        sprite.Pause();
    }

    private void HideChargeIndicator()
    {
        if (ChargeAnimation != null)
        {
            ChargeAnimation.Visible = false;
            ChargeAnimation.Stop();
        }
    }

    private void CancelChargeState()
    {
        _chargeToken++; // cancels any pending await
        _isCharging = false;
        _chargeReady = false;

        HideChargeIndicator();

        if (sprite != null) sprite.SpeedScale = 1f;

        if (currentGun != null)
            sprite?.Play(currentGun.GunId);
    }

    public void Shoot()
    {
        if (currentGun == null || currentGun.CurrentAmmo <= 0)
            return;

        if (Main.Instance.IsWallAt(muzzleFlash.GlobalPosition)) return;

        // If we were paused holding the charge pose, restore speed so shoot anim works
        if (sprite != null) sprite.SpeedScale = 1f;

        currentGun.UseBullet();

        Eventbus.TriggerScreenShake(
            currentGun.ShakeIntensity,
            currentGun.ShakeDuration
        );

        sprite.FireAnimation();
        PlayAnimation();

        Vector2 muzzlePos = muzzleFlash.GlobalPosition;

        float baseAngle = GlobalRotation;
        float halfSpreadRad = Mathf.DegToRad(currentGun.SpreadAngle * 0.5f);

        for (int i = 0; i < currentGun.BulletCount; i++)
        {
            float angle = baseAngle + rng.RandfRange(-halfSpreadRad, halfSpreadRad);

            BulletPool.Spawn(
                position: muzzlePos,
                velocity: new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized(),
                currentGun.BulletData
            );
        }

        _cooldown = currentGun.FireRate;
    }

    private async void PlayAnimation()
    {
        muzzleFlash.Play("default");
        sprite.Play(currentGun.GunId + "Shoot");

        await ToSignal(sprite, "animation_finished");

        // If we’re charging / charged, don't stomp the charge pose.
        if (_isCharging) return;
        if (_chargeReady) return;

        sprite.Play(currentGun.GunId);
    }

    public void AddAnimation(AnimationData AnimationData, string name)
    {
        if (AnimationData == null) return;
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

        // Force charge anims to NOT loop so they reach the end
        bool isChargeAnim = name.EndsWith("Charge", StringComparison.Ordinal);
        sprite.SpriteFrames.SetAnimationLoop(name, isChargeAnim ? false : AnimationData.Looping);
    }
}
