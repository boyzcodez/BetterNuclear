using Godot;
using System;

public partial class GunHandler : Node
{
    [Export] public Guns guns;
    //private UiManager ui;
    private Timer timer;
    private bool locked = false;
    private bool canSwitch = true;

    public override void _Ready()
    {
        //ui = GetTree().GetFirstNodeInGroup("UIManager") as UiManager;
        timer = GetNode<Timer>("Timer");
        timer.Timeout += CanSwitch;
    }

    public override void _UnhandledInput(InputEvent button)
    {
        // if (button.IsActionPressed("tab"))
        // {
        //     ui.wheel.tabPressed = true;
        //     locked = true;

        //     Engine.TimeScale = 0.2f;
        // }
        // else if (button.IsActionReleased("tab"))
        // {
        //     ui.wheel.tabPressed = false;
        //     locked = false;

        //     Engine.TimeScale = 1.0f;
        // }

        if (locked) return;


        if (button is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed && canSwitch)
                {
                    canSwitch = false;
                    guns?.SwitchGuns(1);
                    timer.Start();
                }
                else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed && canSwitch)
                {
                    canSwitch = false;
                    guns?.SwitchGuns(-1);
                    timer.Start();
                }
            }
        
    }
    public override void _Input(InputEvent @event)
    {
        if (locked) return;
        
        if (@event.IsActionPressed("attack") & guns != null)
        {
            guns.shooting = true;
        }
        if (@event.IsActionReleased("attack") & guns != null)
        {
            guns.shooting = false;
        }
    }
    
    public void AddNewGun()
    {
        GD.Print("here will added a new gun?");
    }


    public void CanSwitch()
    {
        canSwitch = true;
    }
}
