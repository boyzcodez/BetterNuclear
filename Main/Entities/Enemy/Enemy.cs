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

    // Stats data
    public float Speed = 30f;
    public float DashSpeed = 200f;
    public float Radius = 12f;

    public bool InSight;
    public Vector2 nextPos;
    public Vector2 playerPos => player.GlobalPosition;
    private IEnemyBehavior behavior;

    public Vector2 Velocity { get; private set; }
    private Vector2 lastPosition;


    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        Connect(SignalName.Activation, new Callable(this, nameof(Activate)));
        Connect(SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        Visible = false;
        SetPhysicsProcess(false);

        ChangeBehavior(new WanderBehavior());

    }
    public override void _PhysicsProcess(double delta)
    {
        lastPosition = GlobalPosition;

        behavior?.Update(this, (float)delta);

        Velocity = (GlobalPosition - lastPosition) / (float)delta;
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
        nextPos = GlobalPosition + velocity * delta;

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
