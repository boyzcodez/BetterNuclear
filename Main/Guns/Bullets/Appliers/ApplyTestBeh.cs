using Godot;
using System;

[GlobalClass]
public partial class ApplyTestBeh : ApplyBehavior
{
    public override IBulletBehavior CreateBehavior()
    {
        return new TestBehavior();
    }
}
