using Godot;

public partial class BetterCamera : Camera2D
{
    [Export] private TextureRect crack1;
    [Export] private TextureRect crack2;
    [Export] private TextureRect crack3;

    [Export] private float MouseLookMaxDistance = 450f;
    [Export] private float MouseLookStrength = 0.2f;   // 0–1 feel
    [Export] private float MouseLookSmoothing = 10f;   // higher = snappier

    [Export] private bool SnapOffsetToPixelGrid = true;

    private Vector2 _mouseLookOffset = Vector2.Zero;

    private Player _player;
    private RandomNumberGenerator _rng;

    private bool _isShaking = false;
    private float _shakeTime = 0f;
    private float _shakeDuration = 0f;
    private float _shakeIntensity = 0f;
    private Vector2 _shakeOffset = Vector2.Zero;

    private const float MAX_SHAKE_INTENSITY = 24f;

    public override void _Ready()
    {
        // Make sure camera updates in sync with CharacterBody2D movement.
        ProcessCallback = Camera2DProcessCallback.Physics;

        // Keep the camera node itself at local origin; we only use Offset.
        Position = Vector2.Zero;

        _player = GetTree().GetFirstNodeInGroup("Player") as Player;

        _rng = new RandomNumberGenerator();
        _rng.Randomize();

        Eventbus.ScreenShake += StartShake;
    }

    public override void _ExitTree()
    {
        // Avoid leaking event subscriptions.
        Eventbus.ScreenShake -= StartShake;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null)
            return;

        float d = (float)delta;

        UpdateCrackVisuals();
        UpdateShake(d);
        UpdateMouseLook(d);

        Vector2 finalOffset = _mouseLookOffset + _shakeOffset;

        if (SnapOffsetToPixelGrid)
            finalOffset = SnapToPixelGrid(finalOffset);

        // Key change: DO NOT move the camera's GlobalPosition.
        // Let parenting handle following, and only offset the view.
        Offset = finalOffset;
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

        float t = _shakeTime / _shakeDuration; // 1 -> 0
        float decay = t; // linear decay
        float curIntensity = _shakeIntensity * decay;

        float x = _rng.RandfRange(-1f, 1f) * curIntensity;
        float y = _rng.RandfRange(-1f, 1f) * curIntensity;
        _shakeOffset = new Vector2(x, y);
    }

    private void UpdateMouseLook(float d)
    {
        // Mouse in world space
        Vector2 mouseWorldPos = GetGlobalMousePosition();

        // Direction from player to mouse
        Vector2 toMouse = mouseWorldPos - _player.GlobalPosition;

        Vector2 targetOffset = Vector2.Zero;
        float len = toMouse.Length();

        if (len >= 16f)
        {
            float distance = Mathf.Min(len, MouseLookMaxDistance);
            targetOffset = toMouse / len * distance * MouseLookStrength;
        }

        // Smooth interpolation (same exponential smoothing you used)
        _mouseLookOffset = _mouseLookOffset.Lerp(
            targetOffset,
            1f - Mathf.Exp(-MouseLookSmoothing * d)
        );
    }

    // Start a screen shake. If already shaking, ignore unless the new one is stronger.
    public void StartShake(float intensity, float duration)
    {
        if (_isShaking && intensity < _shakeIntensity)
            return;

        intensity = Mathf.Min(intensity, MAX_SHAKE_INTENSITY);
        if (intensity <= 0f || duration <= 0f)
            return;

        _isShaking = true;
        _shakeIntensity = intensity;
        _shakeDuration = duration;
        _shakeTime = duration;
    }

    private void UpdateCrackVisuals()
    {
        // Null-safe so the camera works even if you haven’t wired these yet.
        if (crack1 == null || crack2 == null || crack3 == null)
            return;

        float healthPercent = _player.hurtbox.Health / _player.hurtbox.MaxHealth;

        crack1.Visible = healthPercent < 0.7f;
        crack2.Visible = healthPercent < 0.5f;
        crack3.Visible = healthPercent < 0.3f;
    }

    private Vector2 SnapToPixelGrid(Vector2 v)
    {
        // If you use zoom, snapping in world units should respect zoom:
        // world_step = 1 / zoom means 1 screen pixel worth of world movement.
        float stepX = (Zoom.X != 0f) ? (1f / Zoom.X) : 1f;
        float stepY = (Zoom.Y != 0f) ? (1f / Zoom.Y) : 1f;

        return new Vector2(
            Mathf.Round(v.X / stepX) * stepX,
            Mathf.Round(v.Y / stepY) * stepY
        );
    }
}
