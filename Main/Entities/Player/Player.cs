using Godot;
using System;

public partial class Player : Entity
{
    public static Player Instance {get; private set;}
    public Main main;

    private const float Radius = 10f;
    private const float SPEED = 120f;
    private const float DODGE_SPEED = 200f;
    private const float DODGE_DURATION = 0.4f;

    

    public bool isDodging = false;
    private Vector2 dodgeDirection;
    public float dodgeTime = 0f;
    private float dashCooldown = 0.5f;
    private Vector2 Velocity;

    private Node2D warpDashNode;
    private CpuParticles2D dashParticles;
    private Timer dashTimer;

    public override void _Ready()
    {
        dashTimer = GetNode<Timer>("DashCooldown");
        main = GetTree().GetFirstNodeInGroup("Main") as Main;
        active = true;

        hurtbox.Hit += Knockback;
        hurtbox.Death += OnDeath;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!active) return;

        if (dodgeTime > 0f)
        {
            DodgeLogic((float)delta);

        }
        else
        {
            if (KnockbackTime > 0)
            {
                KnockbackTime -= (float)delta;
                Velocity = KnockbackVelocity;
            }
            else Movement((float)delta);
        }

        ApplyMovement((float)delta);
    }

    private bool CanMoveTo(Vector2 targetPos)
    {
        float skin = 0.4f; // try 0.0f to start, then 0.02..0.1
        float r = Radius - skin;

        // 45° point on a circle is (r/√2, r/√2)
        float d = r * 0.70710678f;

        Vector2[] offsets =
        {
            new Vector2( r, 0),
            new Vector2(-r, 0),
            new Vector2(0,  r),
            new Vector2(0, -r),

            new Vector2( d,  d),
            new Vector2(-d,  d),
            new Vector2( d, -d),
            new Vector2(-d, -d),
        };

        foreach (var off in offsets)
        {
            if (Main.Instance.IsWallAt(targetPos + off))
                return false;
        }

        return true;
    }
    private void ApplyMovement(float delta)
    {
        Vector2 pos = GlobalPosition;
        Vector2 move = Velocity * delta;

        if (CanMoveTo(pos + move))
        {
            GlobalPosition = pos + move;
            return;
        }

        bool xFirst = Mathf.Abs(move.X) > Mathf.Abs(move.Y);

        TryAxis(ref pos, move, xFirst);
        TryAxis(ref pos, move, !xFirst);

        GlobalPosition = pos;
    }

    private void TryAxis(ref Vector2 pos, Vector2 move, bool xAxis)
    {
        if (xAxis && move.X != 0)
        {
            Vector2 xPos = pos + new Vector2(move.X, 0);
            if (CanMoveTo(xPos))
                pos.X = xPos.X;
        }
        else if (!xAxis && move.Y != 0)
        {
            Vector2 yPos = pos + new Vector2(0, move.Y);
            if (CanMoveTo(yPos))
                pos.Y = yPos.Y;
        }
    }

    private void Movement(float delta)
    {
        Vector2 direction = Input.GetVector("left", "right", "up", "down");
        Velocity = Velocity.Lerp(direction * SPEED, 1.0f - (float)Mathf.Exp(-25f * GetPhysicsProcessDeltaTime()));

        if (Input.IsActionJustPressed("dodge") && direction != Vector2.Zero && isDodging == false)
        {
            isDodging = true;
            DodgeRoll(direction);
        }
    }

    public void Knockback(Vector2 direction, float force)
    {
        if (force <= 0) return;

        Vector2 knockback = direction * force;

        if (knockback == Vector2.Zero) return;

        KnockbackTime = 0.2f;
        KnockbackVelocity = knockback;
    }

    public void OnDeath()
    {
        Eventbus.ResetGame();
        hurtbox.ResetHealth();
    }

    private void DodgeRoll(Vector2 direction)
    {
        dodgeDirection = direction.Normalized();
        dodgeTime = DODGE_DURATION;
    }
    private void DodgeLogic(float delta)
    {
        float elapsedPercent = 1.0f - (dodgeTime / DODGE_DURATION);
        float currentSpeed = Mathf.Lerp(DODGE_SPEED, DODGE_SPEED * 0.5f, elapsedPercent);

        Velocity = dodgeDirection * currentSpeed;
        dodgeTime -= delta;

        if (dodgeTime <= 0f)
        {
            var dodgeSpeed = Mathf.Lerp(currentSpeed, SPEED, delta * 8f);
            Velocity = dodgeDirection * dodgeSpeed;
            dashTimer.Start(dashCooldown);
        }
    }

    public void _on_dash_cooldown_timeout()
    {
        isDodging = false;
    }
}
