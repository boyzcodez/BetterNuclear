using Godot;

public partial class Ammo : Sprite2D, ICollectable
{
    private Player player;
    private bool EnteredRange = false;
    private float speed = 250f;
    private float pickupThreshold = 10f;
    private float enterThreshold = 150f;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Player;
        SetPhysicsProcess(false);
    }

    public void OnActivation(Vector2 position)
    {
        GlobalPosition = position;

        EnteredRange = false;
        SetPhysicsProcess(true);
        Visible = true;
    }
    public void OnDeactivation()
    {
        Visible = false;
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
        Vector2 dir =( GlobalPosition - player.GlobalPosition).Normalized();

        if (EnteredRange)
        {
            GlobalPosition += dir * speed;
            if (distance < pickupThreshold)
            {
                OnDeactivation();
                GD.Print("Collected Ammo");
            }
        }
        else if (distance < enterThreshold)
        {
            EnteredRange = true;
        }

        
    }

}
