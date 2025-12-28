using Godot;
using System;

public partial class EnemyAnimations : AnimatedSprite2D
{
    [Signal]
    public delegate void AnimationDoneEventHandler();

    [Export] private EnemyLook look;

    private const string RunAnim = "Run";

    private Direction directionNode;

    public float angle;
    private float lastFacingAngle = 0f;

    public int animationPriority = 0;
    private string currentDirection = "Front";
    private string currentAnim = "Idle";

    private Enemy owner;

    public override void _Ready()
    {
        directionNode = GetNode<Direction>("Direction");
        owner = GetOwner<Enemy>();

        AnimationFinished += _on_animation_finished;
        owner.Connect(Enemy.SignalName.Activation, new Callable(this, nameof(Activate)));
        owner.Connect(Enemy.SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        SetPhysicsProcess(false);
    }
    public override void _PhysicsProcess(double delta)
    {
        Vector2 vel = owner.Velocity;

        currentAnim = vel != Vector2.Zero ? RunAnim : "Idle";

        if (owner.InSight)
        {
            angle = look.Rotation;
            lastFacingAngle = angle;
        }
        else if (vel != Vector2.Zero)
        {
            angle = vel.Angle();
            lastFacingAngle = angle;
        }
        else
        {
            angle = lastFacingAngle;
        }
        

        int sectionIndex = (int)(Mathf.Snapped(angle, Mathf.Pi / 4.0f) / (Mathf.Pi / 4.0f));
        sectionIndex = Mathf.Wrap(sectionIndex, 0, 8);

        currentDirection = directionNode.GetDirection(sectionIndex);
        PlayAnimation(currentDirection + currentAnim, 1);
    }

    public void _on_hurtbox_hit(Vector2 direction, float force)
    {
        PlayAnimation(currentDirection + "Hit", 2);
    }
    public void PlayDeath()
    {
        PlayAnimation("Death", 10);
    }

    public void PlayAnimation(string animation, int priority = 0)
    {
        if (priority >= animationPriority)
        {
            animationPriority = priority;
            Play(animation);
        }
    }
    private void _on_animation_finished()
    {
        animationPriority = 0;
        EmitSignal(SignalName.AnimationDone);
    }

    public void Activate()
    {
        SetPhysicsProcess(true);
    }
    public void Deactivate()
    {
        SetPhysicsProcess(false);
    }
}
