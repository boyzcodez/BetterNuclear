using Godot;
using System;

public partial class Dust : Sprite2D
{
    Tween tween;
    public void Play()
    {
        // Random generator
        RandomNumberGenerator rng = new();
        rng.Randomize();

        // Settings
        float duration = rng.RandfRange(0.4f, 0.8f);

        // Random float direction
        Vector2 direction = Vector2.Right
            .Rotated(rng.RandfRange(0, Mathf.Tau))
            * rng.RandfRange(20f, 60f);

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
