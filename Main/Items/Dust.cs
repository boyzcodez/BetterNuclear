using Godot;
using System;

public partial class Dust : Sprite2D
{
    [Export] public float minDuration = 0.4f;
    [Export] public float maxDuration = 0.8f;
    [Export] public float minDistance = 20f;
    [Export] public float maxDistance = 60f;
    Tween tween;
    public void Play()
    {
        // Stop any existing tween and reset state so this sprite always starts
        // from the original settings when Play() is called.
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }

        // Reset transform and modulate to default "spawn" values
        this.Position = Vector2.Zero;
        this.Rotation = 0f;
        this.Scale = Vector2.One;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1f);

        // Random generator
        RandomNumberGenerator rng = new();
        rng.Randomize();

        // Settings
        float duration = rng.RandfRange(minDuration, maxDuration);

        // Random float direction
        Vector2 direction = Vector2.Right
            .Rotated(rng.RandfRange(0, Mathf.Tau))
            * rng.RandfRange(minDistance, maxDistance);

        // Random rotation
        float rotationAmount = rng.RandfRange(-Mathf.Pi, Mathf.Pi);

        // Create tween
        tween = CreateTween();
        tween.SetParallel(true);
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Quad);

        // Move
        tween.TweenProperty(
            this,
            "position",
            this.Position + direction,
            duration
        );

        // Rotate
        tween.TweenProperty(
            this,
            "rotation",
            this.Rotation + rotationAmount,
            duration
        );

        // Scale down
        tween.TweenProperty(
            this,
            "scale",
            Vector2.Zero,
            duration
        );

        // Optional: fade out
        tween.TweenProperty(
            this,
            "modulate:a",
            0f,
            duration
        );
    }
}
