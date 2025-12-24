using Godot;

public interface ICollectable
{
    void OnActivation(Vector2 position);
    void OnDeactivation();
    void QueueFree()
    {
        QueueFree();
    }
}
