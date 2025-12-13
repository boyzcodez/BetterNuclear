using Godot;

public partial class GunAnimation : AnimatedSprite2D
{
    private Vector2 spot;
    private Tween tween;
    private Tween tween2;

    public override void _Ready()
    {
        spot = Position;
    }

    public void FireAnimation()
    {
        // if (tween != null && tween.IsRunning())
        //     tween.Kill();
        if (tween2 != null && tween2.IsRunning())
            tween2.Kill();

        //tween = CreateTween();
        tween2 = CreateTween();

        //tween.TweenProperty(this, "rotation_degrees", -25, 0.05f).SetEase(Tween.EaseType.Out);
        tween2.TweenProperty(this, "position", spot + Vector2.Left * 5, 0.1f).SetEase(Tween.EaseType.Out);

        //tween.TweenProperty(this, "rotation_degrees", 0, 0.1f);
        tween2.TweenProperty(this, "position", spot, 0.1f);
    }

    public void ThrowAnimation()
    {
        if (tween != null && tween.IsRunning())
            tween.Kill();
        if (tween2 != null && tween2.IsRunning())
            tween2.Kill();

        tween = CreateTween();
        tween2 = CreateTween();

        tween.TweenProperty(this, "rotation_degrees", 15, 0.05f).SetEase(Tween.EaseType.Out);
        tween2.TweenProperty(this, "position", Vector2.Right * 3, 0.05f).SetEase(Tween.EaseType.Out);

        tween.TweenProperty(this, "rotation_degrees", 0, 0.1f);
        tween2.TweenProperty(this, "position", Vector2.Zero, 0.1f);
    }
}
