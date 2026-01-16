using Godot;
using System;

public partial class GunHandler : Node
{
    [Export] public Guns guns;
    //private UiManager ui;
    private Timer timer;
    private bool locked = false;
    private bool canSwitch = true;

    private WeaponWheel weaponWheel;

    public override void _Ready()
    {
        weaponWheel = GetTree().GetFirstNodeInGroup("WeaponWheel") as WeaponWheel;
        timer = GetNode<Timer>("Timer");
        timer.Timeout += CanSwitch;

        if (weaponWheel != null)
            foreach (var gun in guns.guns)
            {
                Eventbus.TriggerAddGun(gun);
            }
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
        
        if (Input.IsActionJustPressed("weapon_wheel"))
            weaponWheel.Open();

        if (Input.IsActionJustReleased("weapon_wheel"))
            weaponWheel.Close();
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
