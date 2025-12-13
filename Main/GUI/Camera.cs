using Godot;
using System;

public partial class Camera : Camera2D
{
    private Player player;
    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }
    public override void _Process(double delta)
    {
        GlobalPosition = player.GlobalPosition;
    }


}
