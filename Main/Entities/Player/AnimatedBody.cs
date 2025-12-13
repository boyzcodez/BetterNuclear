using Godot;
using System;

public partial class AnimatedBody : AnimatedSprite2D
{
    [Signal]
    public delegate void AnimationDoneEventHandler();
    public int animationPriority = 0;

    private const string RunAnim = "Run";
    private Player player;

    private Direction directionNode;
    private string currentDirection;
    private string currentAnim = "";

    public override void _Ready()
    {
        directionNode = GetNode<Direction>("Direction");
        player = GetOwner<Player>();
    }
    public override void _Process(double delta)
    {
        HandleMovement();
        HandleAnimation();
    }

    protected void HandleMovement()
    {
        var inputDir = Input.GetVector("left", "right", "up", "down");
        currentAnim = inputDir != Vector2.Zero ? RunAnim : "Idle";
    }

    protected void HandleAnimation()
    {
        Vector2 mouse = GetLocalMousePosition();
        int sectionIndex = (int)(Mathf.Snapped(mouse.Angle(), Mathf.Pi / 4.0f) / (Mathf.Pi / 4.0f));
        sectionIndex = Mathf.Wrap(sectionIndex, 0, 8);

        currentDirection = directionNode.GetDirection(sectionIndex);

        //if (player.Dead) PlayAnimation("Death", 10);
        //else if (warpDash.isWarping) PlayAnimation("Glitch", 1);
        PlayAnimation(currentAnim, 1);
    }

    public void PlayAnimation(string animation, int priority = 0)
    {
        if (priority >= animationPriority)
        {
            animationPriority = priority;
            Play(currentDirection + animation);
        }
    }
    private void _on_animation_finished()
    {
        animationPriority = 0;
        EmitSignal(SignalName.AnimationDone);
    }
}
