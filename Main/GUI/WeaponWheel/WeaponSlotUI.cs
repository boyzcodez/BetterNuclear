using System.ComponentModel;
using Godot;

public partial class WeaponSlotUI : Control
{
    [Export] private TextureRect _icon;
    [Export] private Label _ammo;

    public GunData _gun;

    public void Add(GunData data)
    {
        _gun = data;

        _icon.Texture = _gun.Icon;
        _ammo.Text = _gun.CurrentAmmo + " / " + _gun.MaxAmmo;
    }

    // public GunData Gun
    // {
    //     get => _gun;
    //     set
    //     {
    //         if (_gun == value) return;

    //         // Disconnect old
    //         if (_gun != null)
    //             _gun.Changed -= OnGunChanged;

    //         _gun = value;

    //         // Connect new
    //         if (_gun != null)
    //             _gun.Changed += OnGunChanged;

    //         Refresh();
    //     }
    // }

    // public override void _ExitTree()
    // {
    //     // safety cleanup
    //     if (_gun != null)
    //         _gun.Changed -= OnGunChanged;
    // }

    // private void OnGunChanged() => Refresh();

    // private void Refresh()
    // {
    //     if (_gun == null)
    //     {
    //         if (_icon != null) _icon.Texture = null;
    //         if (_ammo != null) _ammo.Text = "";
    //         return;
    //     }

    //     if (_icon != null) _icon.Texture = _gun.Icon;

    //     if (_ammo != null)
    //         _ammo.Text = $"{_gun.CurrentAmmo}/{_gun.MaxAmmo}";
    // }
}
