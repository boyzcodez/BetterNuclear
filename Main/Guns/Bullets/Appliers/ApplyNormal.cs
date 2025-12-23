using Godot;
using System;

[GlobalClass]
public partial class ApplyNormal : ApplyBehavior
{
    public override IBulletBehavior CreateBehavior()
    {
        return new NormalBullet();
    }
}
