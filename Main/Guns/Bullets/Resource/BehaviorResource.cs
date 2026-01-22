using Godot;

[GlobalClass]
public abstract partial class BehaviorResource : Resource
{
    // Factory: make a fresh runtime instance for each bullet
    public abstract IBulletBehaviorRuntime CreateRuntime();
}
