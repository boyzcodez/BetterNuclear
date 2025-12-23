using Godot;
using System;

[GlobalClass]
public partial class ApplyBounce : ApplyBehavior
{
    [Export] private int amount = 3;
    public override IBulletBehavior CreateBehavior()
    {
        return new BounceBehavior(amount);
    }
}
