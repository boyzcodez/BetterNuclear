using Godot;

public class WanderBehavior : IEnemyBehavior
{
    private RandomNumberGenerator rng = new();
    private Vector2 _dir;

    private float _moveTime;
    private float _waitTime;

    private enum State
    {
        Waiting,
        Moving
    }

    private State _state;

    public void Enter(Enemy enemy)
    {
        rng.Randomize();
        StartWaiting();
    }

    public void Update(Enemy enemy, float delta)
    {
        // Player spotted → switch behavior
        if (enemy.InSight)
        {
            //enemy.ChangeBehavior(new ShootBehavior());
            GD.Print("I should shoot here");
            return;
        }

        switch (_state)
        {
            case State.Waiting:
                _waitTime -= delta;
                if (_waitTime <= 0f)
                    StartMoving();
                break;

            case State.Moving:
                _moveTime -= delta;

                Vector2 velocity = _dir * enemy.Speed;
                enemy.Move(velocity, delta);

                // Hit wall or time over → wait again
                if (_moveTime <= 0f)
                    StartWaiting();

                break;
        }
    }

    public void Exit(Enemy enemy) { }

    private void StartWaiting()
    {
        _state = State.Waiting;
        _waitTime = rng.RandfRange(0.3f, 2.2f);
    }

    private void StartMoving()
    {
        _state = State.Moving;
        _dir = Vector2.Right.Rotated(GD.Randf() * Mathf.Tau);
        _moveTime = rng.RandfRange(0.6f, 3.8f);
    }
}
