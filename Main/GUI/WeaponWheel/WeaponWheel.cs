using Godot;
using System.Collections.Generic;

public partial class WeaponWheel : Control
{
    // --- External assignment ---
    [Export] public PackedScene WeaponSlotScene;
    [Export] public Control SlotsContainer;

    // SubViewport bits
    [Export] public SubViewport WheelViewport;
    [Export] public TextureRect WheelDisplay;
    [Export] public WheelDraw WheelDraw;

    // --- Wheel state exposed for WheelDraw.cs ---
    public bool IsOpen => _open;
    public int HoverIndex => _hoverIndex;
    public int GunCount => _guns.Count;

    // --- Shape ---
    [ExportGroup("Wheel Shape")]
    [Export] public float InnerRadius = 60f;
    [Export] public float OuterRadius = 300f;
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float SlotRadialBias = 0.6f;
    [Export] public float Deadzone = 60f;
    [Export] public float StartAngle = -Mathf.Pi * 0.5f;

    // --- Hover ---
    [ExportGroup("Hover")]
    [Export] public float HoverPush = 10f;
    [Export] public float OutlineWidth = 3f;

    // --- Colors ---
    [ExportGroup("Colors")]
    [Export] public Color SegmentColor = new Color(1, 1, 1, 0.10f);
    [Export] public Color SegmentHoverColor = new Color(1, 1, 1, 0.22f);
    [Export] public Color OutlineColor = new Color(1, 1, 1, 0.12f);
    [Export] public Color OutlineHoverColor = new Color(1, 1, 1, 0.55f);
    [Export] public Color CenterColor = new Color(1, 1, 1, 0.06f);

    [ExportGroup("Rendering")]
    [Export(PropertyHint.Range, "4,64,1")]
    public int WedgeSegments = 24;
    [Export] public bool DrawOutlineAlways = true;

    // --- Pixelation ---
    [ExportGroup("Pixelation")]
    [Export] public bool Pixelate = true;
    [Export(PropertyHint.Range, "1,16,1")]
    public int PixelScale = 4;   // bigger => chunkier pixels
    [Export] public int WheelPaddingPx = 8; // extra space around wheel (in screen pixels)

    // Guns + slots
    private readonly List<GunData> _guns = new();
    private readonly List<WeaponSlotUI> _slotUis = new();

    public int DrawPixelScale => Pixelate ? Mathf.Max(1, PixelScale) : 1;

    private bool _open;
    private int _hoverIndex = -1;

    public override void _Ready()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;

        // Link draw node to this controller
        if (WheelDraw != null)
            WheelDraw.Wheel = this;

        // Make sure the wheel texture scales with nearest
        // (This property exists on CanvasItem in Godot 4)
        if (WheelDisplay != null)
            WheelDisplay.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

        if (WheelViewport != null)
        {
            WheelViewport.TransparentBg = true;
            WheelViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible;
        }

        UpdateWheelRects();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
            UpdateWheelRects();
    }

    // Call this whenever you change radii/pixel settings at runtime
    private void UpdateWheelRects()
    {
        if (WheelViewport == null || WheelDraw == null || WheelDisplay == null || SlotsContainer == null)
            return;

        float displayRadius = OuterRadius + HoverPush + OutlineWidth + WheelPaddingPx;
        Vector2 displaySize = new Vector2(displayRadius * 2f, displayRadius * 2f);
        Vector2 topLeft = (Size - displaySize) * 0.5f;

        // Place the on-screen display
        WheelDisplay.Position = topLeft;
        WheelDisplay.Size = displaySize;

        // Match slots to the same area
        SlotsContainer.Position = topLeft;
        SlotsContainer.Size = displaySize;

        // Set the low-res viewport size
        Vector2 low = Pixelate ? (displaySize / Mathf.Max(1, PixelScale)) : displaySize;
        Vector2I vpSize = new Vector2I(
            Mathf.Max(1, Mathf.RoundToInt(low.X)),
            Mathf.Max(1, Mathf.RoundToInt(low.Y))
        );

        WheelViewport.Size = vpSize;
        WheelDraw.Size = vpSize;

        // Ensure display is showing the viewport texture
        WheelDisplay.Texture = WheelViewport.GetTexture();

        WheelDraw.QueueRedraw();
        UpdateSlotPositions();
    }

    public void SetGuns(List<GunData> guns)
    {
        _guns.Clear();
        _guns.AddRange(guns);
        if (_open) RebuildSlots();
    }

    public void Open()
    {
        if (_guns.Count < 2)
        {
            GD.Print($"Open() guns={_guns.Count}");
            return;
        } 
        _open = true;
        Visible = true;

        UpdateWheelRects();
        RebuildSlots();

        WheelDraw.QueueRedraw();        
    }

    public void Close()
    {
        _open = false;
        Visible = false;
        _hoverIndex = -1;
        WheelDraw.QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (!_open) return;
        UpdateHover();
    }

    private void UpdateHover()
    {
        if (_guns.Count < 2) return;

        // Mouse in global; wheel center is the center of WheelViewportContainer in global space
        Vector2 centerGlobal = WheelDisplay.GlobalPosition + WheelDisplay.Size * 0.5f;
        Vector2 mouse = GetGlobalMousePosition();
        Vector2 toMouse = mouse - centerGlobal;

        float dist = toMouse.Length();
        int newHover = -1;

        if (dist >= Deadzone)
        {
            float ang = Mathf.Atan2(toMouse.Y, toMouse.X); // -π..π
            float t = ((ang - StartAngle) + Mathf.Tau) % Mathf.Tau;

            float step = Mathf.Tau / _guns.Count;
            newHover = Mathf.FloorToInt(t / step);
            newHover = Mathf.Clamp(newHover, 0, _guns.Count - 1);
        }

        if (newHover != _hoverIndex)
        {
            _hoverIndex = newHover;
            UpdateSlotPositions();
            WheelDraw.QueueRedraw();
        }
    }

    private void RebuildSlots()
    {
        if (WeaponSlotScene == null || SlotsContainer == null) return;

        foreach (var ui in _slotUis)
            ui.QueueFree();
        _slotUis.Clear();

        for (int i = 0; i < _guns.Count; i++)
        {
            var node = WeaponSlotScene.Instantiate();
            var ui = node as WeaponSlotUI;
            if (ui == null)
            {
                GD.PushError("WeaponSlotScene root is not WeaponSlotUI.");
                node.QueueFree();
                continue;
            }

            SlotsContainer.AddChild(ui);

            //ui._gun = _guns[i];
            ui.Add(_guns[i]);
            ui.Size = new Vector2(96, 96); // or set in the scene and remove this

            _slotUis.Add(ui);
        }

        UpdateSlotPositions();
    }

    private void UpdateSlotPositions()
    {
        if (SlotsContainer == null) return;
        if (_guns.Count < 2) return;

        Vector2 center = SlotsContainer.Size * 0.5f;
        float step = Mathf.Tau / _guns.Count;

        float baseSlotR = Mathf.Lerp(InnerRadius, OuterRadius, SlotRadialBias);

        for (int i = 0; i < _slotUis.Count; i++)
        {
            var ui = _slotUis[i];
            float mid = StartAngle + step * (i + 0.5f);

            float r = baseSlotR + ((i == _hoverIndex) ? HoverPush : 0f);

            Vector2 offset = new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * r;
            ui.Position = center + offset - ui.Size * 0.5f;
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
        if (_open) RebuildSlots();
        else QueueRedraw(); // optional
    }

    private void OnGunRemovedById(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        // If you're using Name (ResourceName) as ID:
        int idx = _guns.FindIndex(g => g != null && g.ResourceName == id);
        if (idx == -1) return;

        _guns.RemoveAt(idx);

        if (_open) RebuildSlots();
        else QueueRedraw();
    }
}
