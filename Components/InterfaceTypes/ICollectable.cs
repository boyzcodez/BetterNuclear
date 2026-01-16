using System;
using Godot;

public interface ICollectable
{
    void Init(string Name, Items pool);
    void OnActivation();
    void OnDeactivation();
}
