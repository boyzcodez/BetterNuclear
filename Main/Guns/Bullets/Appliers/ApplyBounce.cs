using Godot;
using System;

[GlobalClass]
public partial class ApplyBounce : ApplyBehavior
{
    public override IBulletBehavior CreateBehavior()
    {
        return new BounceBehavior(3);
    }
}
