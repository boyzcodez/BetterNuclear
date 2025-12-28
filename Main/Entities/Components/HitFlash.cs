using Godot;
using System;

public partial class HitFlash : Node
{
    private AnimatedSprite2D parent;
    private Tween tween;

    public override void _Ready()
    {
        parent = GetParent<AnimatedSprite2D>();
    }

    public void _on_hurtbox_hit(Vector2 sm, float mse)
    {
        if (parent == null)
            return;

        if (tween != null && tween.IsValid())
            tween.Kill();

        tween = CreateTween();

        tween.TweenMethod(
            Callable.From<float>(SetShader_BlinkIntensity),
            1.5f,   // from
            0.0f,   // to
            0.2f    // duration
        );

        //ImpactShake();
    }

    private void SetShader_BlinkIntensity(float newValue)
    {
        if (parent.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("blink_intensity", newValue);
        }
    }
   
    public void ImpactShake()
    {
        if (parent == null) return;

        if (tween != null && tween.IsValid())
            tween.Kill();

        Vector2 originalPosition = parent.Position;
        tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.SetEase(Tween.EaseType.InOut);

        for (int i = 0; i < 4; i++)
        {
            tween.TweenCallback(Callable.From(() =>
            {
                parent.Position = originalPosition + (Vector2.Right * GD.Randf() - Vector2.Right * 1.0f) * 3f;
            }));
            tween.TweenInterval(0.05f);
        }

        tween.TweenCallback(Callable.From(() =>
        {
            parent.Position = originalPosition;
        }));
    }
}
