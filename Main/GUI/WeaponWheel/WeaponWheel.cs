using Godot;
using System.Collections.Generic;


public partial class WeaponWheel : Control
{
    [Export] public PackedScene WeaponSlotScene;
    [Export] public Control SlotsContainer;

    [ExportGroup("Wheel Shape")]
    [Export] public float InnerRadius = 60f;   // deadzone/inner circle
    [Export] public float OuterRadius = 300f;  // outside of wheel ring
    [Export] public float SlotRadialBias = 0.5f; // 0 = inner edge, 1 = outer edge

    [ExportGroup("Hover")]
    [Export] public float HoverPush = 10f;         // how far the wedge+icon move out
    [Export] public float OutlineWidth = 3f; 

    [ExportGroup("Colors")]
    [Export] public Color SegmentColor = new Color(1, 1, 1, 0.10f);
    [Export] public Color SegmentHoverColor = new Color(1, 1, 1, 0.22f);
    [Export] public Color OutlineColor = new Color(1, 1, 1, 0.12f);
    [Export] public Color OutlineHoverColor = new Color(1, 1, 1, 0.55f);
    [Export] public Color CenterColor = new Color(1, 1, 1, 0.06f);

    [ExportGroup("Rendering")]
    [Export(PropertyHint.Range, "4,64,1")]
    public int WedgeSegments = 24;

    [Export] public bool DrawOutlineAlways = true; // if false, outlines only on hover

    [Export] public float StartAngle = -Mathf.Pi * 0.5f; // top

    public IReadOnlyList<GunData> Guns => _guns;
    private readonly List<GunData> _guns = new();

    private readonly List<WeaponSlotUI> _slotUis = new();

    private bool _open;
    private int _hoverIndex = -1;

    public override void _Ready()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void SetGuns(List<GunData> guns)
    {
        _guns.Clear();
        _guns.AddRange(guns);
        Rebuild();
    }

    public void Open()
    {
        if (_guns.Count < 2) return; // “can’t open with 0–1 gun”
        _open = true;
        Visible = true;
        Rebuild();
    }

    public void Close()
    {
        _open = false;
        Visible = false;
        _hoverIndex = -1;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (!_open) return;

        UpdateHover();
    }

    private void UpdateHover()
    {
        if (_guns.Count < 2)
        {
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                QueueRedraw();
            }
            return;
        }

        Vector2 center = GetWheelCenterGlobal();
        Vector2 mouse = GetGlobalMousePosition();
        Vector2 toMouse = mouse - center;
        float dist = toMouse.Length();

        int newHover = -1;

        if (dist >= InnerRadius)
        {
            float ang = Mathf.Atan2(toMouse.Y, toMouse.X);     // -π..π
            float t = ((ang - StartAngle) + Mathf.Tau) % Mathf.Tau;

            float step = Mathf.Tau / _guns.Count;
            newHover = Mathf.FloorToInt(t / step);
            newHover = Mathf.Clamp(newHover, 0, _guns.Count - 1);
        }

        if (newHover != _hoverIndex)
        {
            _hoverIndex = newHover;
            UpdateSlotPositions();
            QueueRedraw();
        }
    }

    private Vector2 GetWheelCenterGlobal()
    {
        // Center of this Control in global space
        // If wheel is full-screen, this is viewport center; otherwise it’s your control’s center
        return GlobalPosition + Size * 0.5f;
    }

    private async void Rebuild()
    {
        // Clear old UI
        foreach (var ui in _slotUis)
            ui.QueueFree();
        _slotUis.Clear();

        if (SlotsContainer == null || WeaponSlotScene == null)
        {
            QueueRedraw();
            return;
        }

        if (_guns.Count < 2)
        {
            QueueRedraw();
            return;
        }

        float step = Mathf.Tau / _guns.Count;

        for (int i = 0; i < _guns.Count; i++)
        {
            var ui = WeaponSlotScene.Instantiate() as WeaponSlotUI;
            SlotsContainer.AddChild(ui);
            ui.Add(_guns[i]);
            //ui._gun = _guns[i];

            // Place at mid-angle on the ring
            float slotR = Mathf.Lerp(InnerRadius, OuterRadius, SlotRadialBias);

            float mid = StartAngle + step * (i + 0.5f);
            Vector2 offset = new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * slotR;

            // Size/position: give each slot a rect and center it at the target point
            ui.Size = new Vector2(96, 96);
            ui.Position = (SlotsContainer.Size * 0.5f) + offset - ui.Size * 0.5f;

            _slotUis.Add(ui);
        }

        UpdateSlotPositions();
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_open) return;
        if (_guns.Count < 2) return;

        Vector2 center = Size * 0.5f;
        float step = Mathf.Tau / _guns.Count;

        for (int i = 0; i < _guns.Count; i++)
        {
            float a0 = StartAngle + step * i;
            float a1 = a0 + step;
            float mid = (a0 + a1) * 0.5f;

            bool hovered = (i == _hoverIndex);

            Vector2 push = hovered
                ? new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * HoverPush
                : Vector2.Zero;

            // Fill
            var poly = BuildWedgePolygon(center + push, InnerRadius, OuterRadius, a0, a1, segments: WedgeSegments);
            DrawColoredPolygon(poly, hovered ? SegmentHoverColor : SegmentColor);

            // Outline
            if (DrawOutlineAlways || hovered)
            {
                DrawWedgeOutline(
                    center + push,
                    InnerRadius,
                    OuterRadius,
                    a0,
                    a1,
                    hovered ? OutlineHoverColor : OutlineColor,
                    OutlineWidth,
                    segments: WedgeSegments
                );
            }
        }

        // Center/deadzone visual
        DrawCircle(center, InnerRadius, CenterColor);
    }

    private void DrawWedgeOutline(
        Vector2 center,
        float innerR,
        float outerR,
        float a0,
        float a1,
        Color color,
        float width,
        int segments)
    {
        // Build border points: outer arc a0->a1 then inner arc a1->a0
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

        // Close the loop
        pts.Add(pts[0]);

        DrawPolyline(pts.ToArray(), color, width, true);
    }

    private static Vector2[] BuildWedgePolygon(
        Vector2 center,
        float innerR,
        float outerR,
        float a0,
        float a1,
        int segments)
    {
        // polygon goes along outer arc from a0->a1 then inner arc back a1->a0
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

    private void UpdateSlotPositions()
    {
        if (SlotsContainer == null) return;
        if (_guns.Count < 2) return;

        Vector2 containerCenter = SlotsContainer.Size * 0.5f;
        float step = Mathf.Tau / _guns.Count;

        float baseSlotR = Mathf.Lerp(InnerRadius, OuterRadius, SlotRadialBias);

        for (int i = 0; i < _slotUis.Count; i++)
        {
            var ui = _slotUis[i];
            float mid = StartAngle + step * (i + 0.5f);

            float r = baseSlotR + ((i == _hoverIndex) ? HoverPush : 0f);

            Vector2 offset = new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * r;
            ui.Position = containerCenter + offset - ui.Size * 0.5f;
        }
    }

    public override void _EnterTree()
    {
        Eventbus.GunAdded += OnGunAdded;
        Eventbus.GunRemovedById += OnGunRemovedById;
    }

    public override void _ExitTree()
    {
        Eventbus.GunAdded -= OnGunAdded;
        Eventbus.GunRemovedById -= OnGunRemovedById;
    }


    private void OnGunAdded(GunData gun)
    {
        if (gun == null) return;

        // avoid duplicates (by resource instance)
        if (_guns.Contains(gun)) return;

        _guns.Add(gun);

        // if wheel is open, rebuild immediately
        if (_open) Rebuild();
        else QueueRedraw(); // optional
    }

    private void OnGunRemovedById(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        // If you're using Name (ResourceName) as ID:
        int idx = _guns.FindIndex(g => g != null && g.ResourceName == id);
        if (idx == -1) return;

        _guns.RemoveAt(idx);

        if (_open) Rebuild();
        else QueueRedraw();
    }
}
