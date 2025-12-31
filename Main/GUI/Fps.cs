using Godot;
using System;

public partial class Fps : Label
{
    public override void _Process(double delta)
    {
        Text = Engine.GetFramesPerSecond().ToString() + "__" + Eventbus.activeBullets.ToString();

        //Text = Engine.GetFramesPerSecond().ToString();
    }

}
