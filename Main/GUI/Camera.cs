using Godot;
using System;

public partial class Camera : Camera2D
{
    private Player player;
    private RandomNumberGenerator _rng;

    private bool _isShaking = false;
    private float _shakeTime = 0f;
    private float _shakeDuration = 0f;
    private float _shakeIntensity = 0f;
    private Vector2 _shakeOffset = Vector2.Zero;
    private const float MAX_SHAKE_INTENSITY = 24f;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        _rng = new RandomNumberGenerator();
        _rng.Randomize();

        Eventbus.ScreenShake += StartShake;
    }
    public override void _Process(double delta)
    {
        if (player == null)
            return;

        float d = (float)delta;

        if (_isShaking)
        {
            _shakeTime -= d;
            if (_shakeTime <= 0f)
            {
                _isShaking = false;
                _shakeOffset = Vector2.Zero;
            }
            else
            {
                float t = _shakeTime / _shakeDuration; // 1 -> 0
                float decay = t; // linear decay
                float curIntensity = _shakeIntensity * decay;
                float x = _rng.RandfRange(-1f, 1f) * curIntensity;
                float y = _rng.RandfRange(-1f, 1f) * curIntensity;
                _shakeOffset = new Vector2(x, y);
            }
        }

        GlobalPosition = player.GlobalPosition + _shakeOffset;
    }

    // Start a screen shake. If already shaking, call is ignored.
    public void StartShake(float intensity, float duration)
    {
        if (_isShaking)
            return; // don't apply new shake while shaking

        intensity = Mathf.Min(intensity, MAX_SHAKE_INTENSITY);
        if (intensity <= 0f || duration <= 0f)
            return;

        _isShaking = true;
        _shakeIntensity = intensity;
        _shakeDuration = duration;
        _shakeTime = duration;
    }


}
