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
    

    public Hurtbox hurtbox;

    private IEnemyState currentState;

    
    private float moveSpeed = 60f;
    private Vector2 Velocity;
    private Vector2 move;
    private Vector2 lastSlideDir = Vector2.Zero;


    // Stats data
    public float Speed = 30f;
    public float DashSpeed = 200f;
    public float Radius = 12f;

    public bool InSight;
    public Vector2 playerPos => player.GlobalPosition;
    private IEnemyBehavior behavior;


    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        Connect(SignalName.Activation, new Callable(this, nameof(Activate)));
        Connect(SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        Visible = false;
        SetPhysicsProcess(false);

        //currentState = new ChaseState();
        ChangeBehavior(new WanderBehavior());

    }
    public override void _PhysicsProcess(double delta)
    {
        // Vector2 desiredDir = currentState.GetDesiredDirection(this);
        // Vector2 seperation = GetSeparationForce() * 1.2f; // norm 1.2f
        // Vector2 velocity = desiredDir + seperation;

        // if (velocity.LengthSquared() > 0.001f) velocity = velocity.Normalized() * moveSpeed;

        // velocity = SlideAlongWalls(velocity, (float)delta);

        // this technically makes it better at not sliding through walls but i dont know

        // int steps = Mathf.CeilToInt(move.Length() / (Radius * 0.5f));
        // steps = Mathf.Max(1, steps);

        // Vector2 stepVel = velocity / steps;
        // for (int i = 0; i < steps; i++)
        // {
        //     Vector2 slid = SlideAlongWalls(stepVel, (float)delta);
        //     GlobalPosition += slid * (float)delta;
        // }
        

        // before changing position, might need to check that enemies dont 
        // push eachother through walls



        // Vector2 dir = Main.Instance.GetSmoothDirection(GlobalPosition);

        // Vector2 tangent = new Vector2(-dir.Y, dir.X);
        // dir += tangent * Mathf.Sin(Time.GetTicksMsec() * 0.002f) * 0.15f;

        // Vector2 desiredVelocity = dir.Normalized() * moveSpeed;

        // Velocity = Velocity.Lerp(desiredVelocity, 8f * (float)delta);


        // Vector2 dir = Main.Instance.GetDirection(GlobalPosition);
        // Velocity = dir * moveSpeed;
        
        //GlobalPosition += Velocity * (float)delta;

        behavior?.Update(this, (float)delta);
    }


    public void ChangeBehavior(IEnemyBehavior newBehavior)
    {
        behavior?.Exit(this);
        behavior = newBehavior;
        behavior.Enter(this);
    }

    public bool CanMoveTo(Vector2 targetPos)
{
        float r = Radius;

        Vector2[] offsets =
        {
            new Vector2( r, 0),
            new Vector2(-r, 0),
            new Vector2(0,  r),
            new Vector2(0, -r)
        };

        foreach (var off in offsets)
        {
            if (Main.Instance.IsWallAt(targetPos + off))
                return false;
        }

        return true;
    }
    public void Move(Vector2 velocity, float delta)
    {
        Vector2 nextPos = GlobalPosition + velocity * delta;

        if (CanMoveTo(nextPos)) GlobalPosition = nextPos;
    }
    public Vector2 DirectionToPlayer()
    {
        return (playerPos - GlobalPosition).Normalized();
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

    private bool IsWallAtOffset(Vector2 pos, Vector2 dir, float radius)
    {
        if (dir.LengthSquared() < 0.0001f) return false;

        Vector2 checkPos = pos + dir.Normalized() * radius;
        return Main.Instance.IsWallAt(checkPos);
    }

    private Vector2 SlideAlongWalls(Vector2 desiredVelocity, float delta)
    {
        Vector2 pos = GlobalPosition;
        move = desiredVelocity * delta;

        if (move.LengthSquared() < 0.0001f) return Vector2.Zero;

        if (!IsWallAtOffset(pos + move, move, Radius))
            return desiredVelocity;

        // X
        Vector2 xMove = new Vector2(move.X, 0);
        if (xMove.LengthSquared() > 0.0001f &&
            !IsWallAtOffset(pos + xMove, xMove, Radius))
        {
            lastSlideDir = Vector2.Right * Mathf.Sign(desiredVelocity.X);
            return new Vector2(desiredVelocity.X, 0);
        }

        // Y
        Vector2 yMove = new Vector2(0, move.Y);
        if (yMove.LengthSquared() > 0.0001f &&
            !IsWallAtOffset(pos + yMove, yMove, Radius))
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
