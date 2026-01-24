using Godot;

public enum EnemyActions
{
    Shoot,
    Ability,
    Nothing
}

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

    [Export] public EnemyActions action = EnemyActions.Nothing;
    [Export] public Guns gun;
    [Export] public Node2D ability;
    [Export] public ItemResource[] items = [];

    
    public string name;
    public EnemyPool pool {get; set;}
    public Player player;

    // Stats data
    [Export] public float Speed = 30f;
    [Export] public float DashSpeed = 200f;
    [Export] public float Radius = 10f;

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

    public void Move(Vector2 velocity, float delta)
    {
        Vector2 pos = GlobalPosition;
        Vector2 vel = velocity;

        MathCollision.MoveCircle(ref pos, ref vel, Radius, delta);

        GlobalPosition = pos;
        // If you want enemy AI to keep its original intended velocity (not the slid one),
        // don't write vel back anywhere.
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


    public void TriggerAction(EnemyActions trigger)
    {
        switch (trigger)
        {
            case EnemyActions.Shoot:
                gun?.Shoot();
                break;
            case EnemyActions.Ability:
                GD.Print("Fix ability dumbass");
                break;
            case EnemyActions.Nothing:
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

        if (items.Length > 0 ) Eventbus.TriggerSpawnItem(items[0].Id, GlobalPosition);
        
        pool.Return();
    }


    public void Activate()
    {
        hurtbox.ResetHealth();
        
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
