using Godot;

public class DamageData
{
    public int Damage { get; }
    public float Knockback { get; }
    public Node Source { get; }
    public string WeaponName { get; }

    public DamageData(int damage, float knockback, string name)
    {
        Damage = damage;
        Knockback = knockback;
        WeaponName = name;
    }
}
