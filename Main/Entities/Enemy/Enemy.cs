using Godot;
using System;

public partial class Enemy : Node2D
{
    [Signal] public delegate void ActivationEventHandler();
    [Signal] public delegate void DeactivationEventHandler();

    public bool active = false;
    public string name;
    public EnemyPool pool {get; set;}
    public Player player;
    public Vector2 playerPos => player.GlobalPosition;

    public Hurtbox hurtbox;

    private IEnemyState currentState;

    private float Radius = 6f;
    private float moveSpeed = 60f;
    private Vector2 Velocity;
    Vector2 lastSlideDir = Vector2.Zero;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        Connect(SignalName.Activation, new Callable(this, nameof(Activate)));
        Connect(SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        Visible = false;
        SetPhysicsProcess(false);

        currentState = new ChaseState();
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     Vector2 desired = currentState.GetDesiredDirection(this);

    //     Vector2 wallAvoid = GetWallAvoidance(desired) * 1.5f;
    //     Vector2 separation = GetSeparationForce() * 1.2f;

    //     Vector2 finalDir = 
    //         desired +
    //         wallAvoid +
    //         separation;

    //     if (finalDir.LengthSquared() > 0.001f) finalDir = finalDir.Normalized();

    //     Velocity = finalDir * moveSpeed;
    //     GlobalPosition += Velocity * (float)delta;
    // }
    public override void _PhysicsProcess(double delta)
    {
        Vector2 desiredDir = currentState.GetDesiredDirection(this);
        Vector2 seperation = GetSeparationForce() * 1.2f;
        Vector2 velocity = desiredDir + seperation;

        if (velocity.LengthSquared() > 0.001f) velocity = velocity.Normalized() * moveSpeed;

        velocity = SlideAlongWalls(velocity, (float)delta);

        GlobalPosition += velocity * (float)delta;
    }


    Vector2 GetWallAvoidance(Vector2 desiredDir)
    {
        Vector2 avoidance = Vector2.Zero;

        float probeDist = 24f;
        float sideAngle = Mathf.Pi / 4f;

        Vector2[] probes =
        {
            desiredDir,
            desiredDir.Rotated(sideAngle),
            desiredDir.Rotated(-sideAngle)
        };

        foreach (var dir in probes)
        {
            Vector2 probePos = GlobalPosition + dir * probeDist;
            if (Main.Instance.IsWallAt(probePos))
            {
                avoidance -= dir;
            }
        }

        return avoidance;
    }

    Vector2 GetSeparationForce()
    {
        Vector2 force = Vector2.Zero;
        float desiredDist = 20f;

        foreach (var obj in Main.Instance.grid.QueryNearby(GlobalPosition))
        {
            if (obj == this) continue;
            if (obj is not Hurtbox other) continue;

            Vector2 diff = GlobalPosition - other.GlobalPosition;
            float dist = diff.Length();

            if (dist > 0 && dist < desiredDist)
            {
                force += diff.Normalized() * (desiredDist - dist) / desiredDist;
            }
        }

        return force;
    }

    private bool IsBlocked(Vector2 from, Vector2 move)
    {
        Vector2 target = from + move;
        return Main.Instance.IsWallAt(target);
    }
    private bool IsWallAtOffset(Vector2 pos, Vector2 dir, float radius)
    {
        return Main.Instance.IsWallAt(pos + dir.Normalized() * radius);
    }

    private Vector2 SlideAlongWalls(Vector2 desiredVelocity, float delta)
    {
        Vector2 pos = GlobalPosition;
        Vector2 move = desiredVelocity * delta;

        // Try full move first
        if (!Main.Instance.IsWallAt(pos + move))
            return desiredVelocity;

        // Try X only
        Vector2 xMove = new Vector2(move.X, 0);
        if (!Main.Instance.IsWallAt(pos + xMove))
        {
            lastSlideDir = Vector2.Right * Mathf.Sign(desiredVelocity.X);
            return new Vector2(desiredVelocity.X, 0);
        }
            

        // Try Y only
        Vector2 yMove = new Vector2(0, move.Y);
        if (!Main.Instance.IsWallAt(pos + yMove))
        {
            lastSlideDir = Vector2.Down * Mathf.Sign(desiredVelocity.Y);
            return new Vector2(0, desiredVelocity.Y);
        }
            

        // Fully blocked
        return lastSlideDir * moveSpeed * 0.3f;
    }


    public void Activate()
    {
        active = true;
        hurtbox.active = true;
        Visible = true;

        SetPhysicsProcess(true);
    }
    public void Deactivate()
    {
        active = false;
        hurtbox.active = false;
        Visible = false;

        SetPhysicsProcess(false);
    }

}
