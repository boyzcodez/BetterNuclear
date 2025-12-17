using Godot;
using System.Collections.Generic;

public partial class Guns : Node2D
{
    [Export] public GunData[] guns { get; set; } = [];
    [Export] public bool active = false;
    public GunAnimation sprite { get; set; }

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

        //EventBus.Reset += ReFillGuns;

        foreach (var gunData in guns)
        {
            var id = gunData.Name + GetInstanceId();
            pool?.PreparePool(id, gunData);
            
            //if (gunData.Sound != null) AudioLibrary.Add(gunData.GunName, gunData.Sound);

            if (gunData.UsesAnimations)
            {
                AddAnimation(gunData.NormalAnimationData, gunData.Name);
                AddAnimation(gunData.ShootAnimationData, gunData.Name + "Shoot");
            }
        }

        EquipGun(0);
        sprite?.Play(currentGun.Name);

        // add to exp list so that when an enemy dies the correct gun will the the exp
        //if (!guns[_currentGunIndex].isEnemy) XpHandler.AddGun(guns[_currentGunIndex].GunName, this);
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
        id = currentGun.Name + GetInstanceId();

        sprite?.Play(currentGun.Name);
        muzzleFlash.Position = currentGun.ShootPosition;
        Position = new Vector2(currentGun.GunSpot_X_axis, 0);
        if (AudioLibrary.ContainsKey(currentGun.Name)) audioSystem.Stream = AudioLibrary[currentGun.Name];
    }

    public override void _Process(double delta)
    {
        if (shooting && _cooldown <= 0) Shoot();
        else if (_cooldown > 0) _cooldown -= (float)delta;

        if (GlobalRotation > -1.5f && GlobalRotation < 1.5f)
        {
            shaderMaterial.SetShaderParameter("flip_v", false);
            //muzzleFlash.Position = new Vector2(currentGun.ShootPosition.X, currentGun.ShootPosition.Y);
        }
        else
        {
            shaderMaterial.SetShaderParameter("flip_v", true);
            //muzzleFlash.Position = new Vector2(currentGun.ShootPosition.X, -currentGun.ShootPosition.Y);
        } 
    }

    public void Shoot()
    {
        if (currentGun.CurrentAmmo <= 0) return;
        else currentGun.UseBullet();

        if (currentGun == null) return;
        
        sprite.FireAnimation();
        PlayAnimation();
        if (AudioLibrary.ContainsKey(currentGun.Name)) audioSystem.Play();
        else GD.Print("This gun has no Sound");


        Vector2 baseDirection = Vector2.Right.Rotated(GlobalRotation);

        float spreadRad = Mathf.DegToRad(currentGun.SpreadAngle);
        float angleStep = currentGun.BulletCount > 1 ? spreadRad / (currentGun.BulletCount - 1) : 0f;

        for (int i = 0; i < currentGun.BulletCount; i++)
        {
            float angleOffset = -spreadRad / 2f + i * angleStep;
            //Vector2 direction = baseDirection.Rotated(angleOffset); 
            float rotation = GlobalRotation + angleOffset + (float)GD.RandRange(-currentGun.RandomFactor, currentGun.RandomFactor);

            Bullet bullet = pool.GetBullet(id, currentGun);
            bullet.GlobalPosition = muzzleFlash.GlobalPosition + new Vector2(10 * NumBet(currentGun.RandomFactor), 10* NumBet(currentGun.RandomFactor));
            bullet.Velocity = (GetGlobalMousePosition() - GlobalPosition).Normalized() * currentGun.BulletSpeed;

            //if (currentGun.rotate) bullet.Rotation = rotation;

            bullet.Activate();
        }

        _cooldown = currentGun.FireRate;
    }
    private async void PlayAnimation()
    {
        muzzleFlash.Play("default");
        sprite.Play(currentGun.Name + "Shoot");

        await ToSignal(sprite, "animation_finished");

        sprite.Play(currentGun.Name);
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
