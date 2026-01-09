using Godot;

public class DamageData
{
    public float Damage { get; }
    public float Knockback { get; }
    public Node Source { get; }
    public string WeaponName { get; }
    public string DamageType {get;}

    public DamageData(float damage, float knockback, string name, string type)
    {
        Damage = damage;
        Knockback = knockback;
        WeaponName = name;
        DamageType = type;
    }
}
