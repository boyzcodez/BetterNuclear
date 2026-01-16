using Godot;
using System.Collections.Generic;

public static class WeaponWheelLayout
{
    public static List<WheelSlot> BuildSlots(int weaponCount, float startAngle = -Mathf.Pi * 0.5f)
    {
        var slots = new List<WheelSlot>(weaponCount);
        if (weaponCount < 2) return slots;

        float step = Mathf.Tau / weaponCount; // 2π / N
        for (int i = 0; i < weaponCount; i++)
        {
            float a0 = startAngle + step * i;
            float a1 = a0 + step;
            slots.Add(new WheelSlot(i, a0, a1));
        }
        return slots;
    }
    
}
