using Godot;
using System.Collections.Generic;

public partial class WheelDraw : Control
{
    public WeaponWheel Wheel; // assigned by WeaponWheel at runtime

    public override void _Draw()
    {
        if (Wheel == null) return;
        if (!Wheel.IsOpen) return;
        int count = Wheel.GunCount;
        if (count < 2) return;

        int ps = Wheel.DrawPixelScale; // 1 if not pixelating
        float innerR = Wheel.InnerRadius / ps;
        float outerR = Wheel.OuterRadius / ps;
        float hoverPush = Wheel.HoverPush / ps;
        float outlineW = Wheel.OutlineWidth / ps;

        Vector2 center = Size * 0.5f;
        float step = Mathf.Tau / count;

        for (int i = 0; i < count; i++)
        {
            float a0 = Wheel.StartAngle + step * i;
            float a1 = a0 + step;
            float mid = (a0 + a1) * 0.5f;

            bool hovered = (i == Wheel.HoverIndex);

            Vector2 push = hovered
                ? new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * hoverPush
                : Vector2.Zero;

            var poly = BuildWedgePolygon(center + push, innerR, outerR, a0, a1, Wheel.WedgeSegments);
            DrawColoredPolygon(poly, hovered ? Wheel.SegmentHoverColor : Wheel.SegmentColor);

            if (Wheel.DrawOutlineAlways || hovered)
            {
                DrawWedgeOutline(center + push, innerR, outerR, a0, a1,
                    hovered ? Wheel.OutlineHoverColor : Wheel.OutlineColor,
                    outlineW,
                    Wheel.WedgeSegments);
            }
        }

        DrawCircle(center, innerR, Wheel.CenterColor);
    }

    private static Vector2[] BuildWedgePolygon(Vector2 center, float innerR, float outerR, float a0, float a1, int segments)
    {
        var points = new List<Vector2>(segments * 2 + 2);

        for (int s = 0; s <= segments; s++)
        {
            float t = (float)s / segments;
            float a = Mathf.Lerp(a0, a1, t);
            points.Add(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * outerR);
        }

        for (int s = segments; s >= 0; s--)
        {
            float t = (float)s / segments;
            float a = Mathf.Lerp(a0, a1, t);
            points.Add(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * innerR);
        }

        return points.ToArray();
    }

    private void DrawWedgeOutline(Vector2 center, float innerR, float outerR, float a0, float a1, Color color, float width, int segments)
    {
        var pts = new List<Vector2>(segments * 2 + 3);

        for (int s = 0; s <= segments; s++)
        {
            float t = (float)s / segments;
            float a = Mathf.Lerp(a0, a1, t);
            pts.Add(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * outerR);
        }

        for (int s = 0; s <= segments; s++)
        {
            float t = (float)s / segments;
            float a = Mathf.Lerp(a1, a0, t);
            pts.Add(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * innerR);
        }

        pts.Add(pts[0]);
        DrawPolyline(pts.ToArray(), color, width, false);
    }
}
