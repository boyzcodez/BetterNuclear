using Godot;
using System;

[GlobalClass]
public partial class ApplyBehavior : Resource
{
    public virtual IBulletBehavior CreateBehavior()
    {
        return null;
    }
}
