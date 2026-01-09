using Godot;
using System;

public partial class Crater : Sprite2D, ICollectable
{
    public string _Name;
    public Items _Pool;
    public void Init(string Name, Items pool)
    {
        _Name = Name;
        _Pool = pool;

        Eventbus.GenerateMap += OnDeactivation;
        Eventbus.Reset += OnDeactivation;

        //Rotation = (float)GD.Randf() * (float)Math.PI / 6f - (float)Math.PI / 12f;
        Rotation = (float)GD.Randf() * (float)Math.PI / 3f - (float)Math.PI / 6f;
    }
    public void OnActivation()
    {
        
    }
    public void OnDeactivation()
    {
        _Pool.ReturnItem(_Name, this);
    }
}
