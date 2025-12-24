using Godot;

public class WanderBehavior : IEnemyBehavior
{
    private readonly RandomNumberGenerator _rng = new();

    private enum State
    {
        Waiting,
        Moving
    }

    private State _state;

    private Vector2 _direction;

    private float _moveTime;
    private float _waitTime;

    private float _attackCooldown;

    public void Enter(Enemy enemy)
    {
        _rng.Randomize();

        ResetAttackCooldown();
        StartWaiting();
    }

    public void Update(Enemy enemy, float delta)
    {
        UpdateAttackCooldown(delta);

        // -attack
        if (enemy.InSight && _attackCooldown <= 0f)
        {
            enemy.TriggerAction("Shoot");
            ResetAttackCooldown();
        }

        // movement
        switch (_state)
        {
            case State.Waiting:
                _waitTime -= delta;
                if (_waitTime <= 0f)
                    StartMoving();
                break;

            case State.Moving:
                _moveTime -= delta;

                enemy.Move(_direction * enemy.Speed, delta);

                if (_moveTime <= 0f)
                    StartWaiting();
                break;
        }
    }

    public void Death(Enemy enemy)
    {
        ResetAttackCooldown();
        Eventbus.TriggerSpawnItem("Coin", enemy.GlobalPosition);
    }

    
    // functionality
    private void StartWaiting()
    {
        _state = State.Waiting;
        _waitTime = _rng.RandfRange(0.3f, 2.2f);
    }

    private void StartMoving()
    {
        _state = State.Moving;
        _direction = Vector2.Right.Rotated(_rng.RandfRange(0f, Mathf.Tau));
        _moveTime = _rng.RandfRange(0.6f, 3.8f);
    }

    private void ResetAttackCooldown()
    {
        _attackCooldown = _rng.RandfRange(1.2f, 4.5f);
    }

    private void UpdateAttackCooldown(float delta)
    {
        if (_attackCooldown > 0f)
            _attackCooldown -= delta;
    }
}
