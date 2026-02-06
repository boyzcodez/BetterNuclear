using Godot;

public partial class SubpixelCamera : Camera2D
{
    [Export] private Player player;
    [Export] private SubViewportContainer viewportContainer;

    // Optional overlay cracks (null-safe)
    [Export] private TextureRect crack1;
    [Export] private TextureRect crack2;
    [Export] private TextureRect crack3;

    // Follow smoothing (your existing value was delta * 3)
    [Export] private float FollowSmoothing = 5f;

    // Mouse look
    [Export] private float MouseLookMaxDistance = 450f;
    [Export] private float MouseLookStrength = 0.2f;   // 0–1 feel
    [Export] private float MouseLookSmoothing = 10f;   // higher = snappier
    [Export] private float MouseDeadZone = 16f;

    // Shake
    [Export] private float MaxShakeIntensity = 24f;

    private Vector2 _actualPos;
    private Vector2 _mouseLookOffset = Vector2.Zero;

    private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    private bool _isShaking = false;
    private float _shakeTime = 0f;
    private float _shakeDuration = 0f;
    private float _shakeIntensity = 0f;
    private Vector2 _shakeOffset = Vector2.Zero;

    public override void _Ready()
    {
        ProcessCallback = Camera2DProcessCallback.Physics;

        _rng.Randomize();

        // Start camera at player position if available, so it doesn't jump on first frame
        if (player != null)
            _actualPos = player.GlobalPosition;

        Eventbus.ScreenShake += StartShake;
    }

    public override void _ExitTree()
    {
        Eventbus.ScreenShake -= StartShake;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null)
            return;

        float d = (float)delta;

        UpdateCrackVisuals();
        UpdateShake(d);
        UpdateMouseLook(d);

        // Target world position includes mouse look + shake, but stays in world-space
        Vector2 targetWorldPos = player.GlobalPosition + _mouseLookOffset + _shakeOffset;

        // Smooth follow in "actual" (subpixel) space
        float t = 1f - Mathf.Exp(-FollowSmoothing * d);
        _actualPos = _actualPos.Lerp(targetWorldPos, t);

        // Subpixel workflow: round for actual camera position, send fractional offset to shader
        Vector2 rounded = _actualPos.Round();
        Vector2 camSubpixelOffset = rounded - _actualPos;

        if (viewportContainer?.Material is ShaderMaterial shaderMaterial)
            shaderMaterial.SetShaderParameter("cam_offset", camSubpixelOffset);

        GlobalPosition = rounded;
    }

    private void UpdateMouseLook(float d)
    {
        // Mouse in world space
        Vector2 mouseWorldPos = GetGlobalMousePosition();

        // Direction from player to mouse
        Vector2 toMouse = mouseWorldPos - player.GlobalPosition;

        Vector2 targetOffset = Vector2.Zero;
        float len = toMouse.Length();

        if (len >= MouseDeadZone)
        {
            float distance = Mathf.Min(len, MouseLookMaxDistance);
            targetOffset = (toMouse / len) * distance * MouseLookStrength;
        }

        float t = 1f - Mathf.Exp(-MouseLookSmoothing * d);
        _mouseLookOffset = _mouseLookOffset.Lerp(targetOffset, t);
    }

    private void UpdateShake(float d)
    {
        if (!_isShaking)
            return;

        _shakeTime -= d;
        if (_shakeTime <= 0f)
        {
            _isShaking = false;
            _shakeOffset = Vector2.Zero;
            return;
        }

        // Linear decay like your old camera
        float t = _shakeTime / _shakeDuration; // 1 -> 0
        float curIntensity = _shakeIntensity * t;

        float x = _rng.RandfRange(-1f, 1f) * curIntensity;
        float y = _rng.RandfRange(-1f, 1f) * curIntensity;
        _shakeOffset = new Vector2(x, y);
    }

    // Start a screen shake. If already shaking, ignore unless the new one is stronger.
    public void StartShake(float intensity, float duration)
    {
        if (_isShaking && intensity < _shakeIntensity)
            return;

        intensity = Mathf.Min(intensity, MaxShakeIntensity);
        if (intensity <= 0f || duration <= 0f)
            return;

        _isShaking = true;
        _shakeIntensity = intensity;
        _shakeDuration = duration;
        _shakeTime = duration;
    }

    private void UpdateCrackVisuals()
    {
        if (crack1 == null || crack2 == null || crack3 == null)
            return;

        // Keep null-safe assumptions about your Player/hurtbox structure
        if (player?.hurtbox == null || player.hurtbox.MaxHealth <= 0f)
            return;

        float healthPercent = player.hurtbox.Health / player.hurtbox.MaxHealth;

        crack1.Visible = healthPercent < 0.7f;
        crack2.Visible = healthPercent < 0.5f;
        crack3.Visible = healthPercent < 0.3f;
    }
}
