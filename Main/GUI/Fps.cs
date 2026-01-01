using Godot;
using System;

public partial class Fps : Label
{
    public override void _Process(double delta)
    {
        Text = Engine.GetFramesPerSecond().ToString() + " fps __  " + Eventbus.activeBullets.ToString() + " bullets";

        //Text = Engine.GetFramesPerSecond().ToString();
    }

}
