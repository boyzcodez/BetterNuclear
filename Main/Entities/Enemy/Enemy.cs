using Godot;
using System;

public partial class Enemy : Entity
{
    [Signal] public delegate void ActivationEventHandler();
    [Signal] public delegate void DeactivationEventHandler();

    public enum AttackType
    {
        Shoot,
        Ability,
        Nothing
    }

    [Export(PropertyHint.Enum, "Shoot,Ability,Nothing")]
    public string Trigger { get; set; } = "Shoot";
    [Export] public Guns gun;
    [Export] public Node2D ability;

    
    public string name;
    public EnemyPool pool {get; set;}
    public Player player;

    // Stats data
    [Export] public float Speed = 30f;
    [Export] public float DashSpeed = 200f;
    [Export] public float Radius = 15f;

    public bool InSight;
    public Vector2 nextPos;
    public Vector2 playerPos => player.GlobalPosition;
    private IEnemyBehavior behavior;

    public Vector2 Velocity { get; private set; }
    private Vector2 lastPosition;
    public float KnockbackTime = 0f;
    public Vector2 KnockbackVelocity;


    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;

        Connect(SignalName.Activation, new Callable(this, nameof(Activate)));
        Connect(SignalName.Deactivation, new Callable(this, nameof(Deactivate)));

        hurtbox.Hit += Knockback;
        hurtbox.Death += OnDeath;

        active = false;
        Visible = false;
        SetPhysicsProcess(false);

        behavior = new WanderBehavior();
    }
    public override void _PhysicsProcess(double delta)
    {
        lastPosition = GlobalPosition;

        if (KnockbackTime > 0)
        {
            KnockbackTime -= (float)delta;
            Move(KnockbackVelocity, (float)delta);

            return;
        }

        behavior?.Update(this, (float)delta);

        Velocity = (GlobalPosition - lastPosition) / (float)delta;
    }

    public bool CanMoveTo(Vector2 targetPos)
    {
        //float r = Radius; 

        // this i think fixes corners
        float padding = 0.5f;
        float r = Radius - padding;

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
        Vector2 pos = GlobalPosition;
        Vector2 desiredMove = velocity * delta;

        // Try X movement
        if (desiredMove.X != 0)
        {
            Vector2 xMovePos = pos + new Vector2(desiredMove.X, 0);
            if (CanMoveTo(xMovePos))
                pos.X = xMovePos.X;
        }

        // Try Y movement
        if (desiredMove.Y != 0)
        {
            Vector2 yMovePos = pos + new Vector2(0, desiredMove.Y);
            if (CanMoveTo(yMovePos))
                pos.Y = yMovePos.Y;
        }

        GlobalPosition = pos;
    }
    public Vector2 DirectionToPlayer()
    {
        return (playerPos - GlobalPosition).Normalized();
    }


    public void Knockback(Vector2 direction, float force)
    {
        if (force <= 0) return;

        Vector2 knockback = direction * force;

        if (knockback == Vector2.Zero) return;

        KnockbackTime = 0.2f;
        KnockbackVelocity = knockback;
    }


    public void TriggerAction(string trigger)
    {
        switch (trigger)
        {
            case "Shoot":
                gun?.Shoot();
                break;
            case "Ability":
                GD.Print("Fix ability dumbass");
                break;
            case "Nothing":
                GD.Print("I will do nothing");
                break;
        }
    }


    // might use this to seperate enemies
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

    public void OnDeath()
    {
        if (active) behavior?.Death(this);
        EmitSignal("Deactivation");
        pool.Return();
    }


    public void Activate()
    {
        active = true;
        Visible = true;

        SetPhysicsProcess(true);
    }
    public void Deactivate()
    {
        active = false;

        SetPhysicsProcess(false);
    }

}
