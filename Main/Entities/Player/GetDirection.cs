using Godot;

public partial class GetDirection : Node2D
{
    [Export] private Look look;

    public override void _Process(double delta)
    {
        if (look == null) return;

        Vector2 toMouse = GetGlobalMousePosition() - GlobalPosition;

        look.SetRotation(toMouse);
    }

}
