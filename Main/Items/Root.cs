using Godot;
using System.Collections.Generic;

public partial class Root : Node2D
{
    /*
     * Procedural tentacle arm showcasing FABRIK IK with a wave motion.
     *
     * Multi-pass approach:
     * 1. FABRIK IK for accurate targeting
     * 2. Constraints to prevent stretching/compression
     * 3. Wave motion for organic feel
     * 4. Final constraint pass
     */

    #region Exports

    [ExportGroup("Node References")]

    [Export]
    public Line2D BaseNode
    {
        get => _baseNode;
        set
        {
            _baseNode = value;
            if (_baseNode != null)
                _basePosition = _baseNode.Position;

            ApplyLineWidth();
            ApplyWidthCurve();
            InitializeSegments();
        }
    }
    private Line2D _baseNode;

    [Export]
    public Line2D ShadowNode
    {
        get => _shadowNode;
        set
        {
            _shadowNode = value;
            ApplyLineWidth();
            ApplyWidthCurve();
        }
    }
    private Line2D _shadowNode;

    [Export]
    public Node2D Target;

    [ExportGroup("IK Configuration")]

    [Export(PropertyHint.Range, "3,50,1")]
    public int NumSegments
    {
        get => _numSegments;
        set
        {
            _numSegments = value;
            InitializeSegments();
        }
    }
    private int _numSegments = 24;

    [Export(PropertyHint.Range, "10,128,1")]
    public float MaxLength
    {
        get => _maxLength;
        set
        {
            _maxLength = value;
            InitializeSegments();
        }
    }
    private float _maxLength = 128f;

    [Export(PropertyHint.Range, "1,10,1")]
    public int IkIterations = 2;

    [Export(PropertyHint.Range, "1,20,1")]
    public int ConstraintIterations = 10;

    [Export]
    public bool EnableConstraint = true;

    [ExportGroup("Wave Motion")]

    [Export(PropertyHint.Range, "0,5,0.5")]
    public float WaveAmplitude = 2.5f;

    [Export(PropertyHint.Range, "0,5,0.1")]
    public float WaveFrequency = 2.0f;

    [Export(PropertyHint.Range, "0,10,0.1")]
    public float WaveSpeed = 3.0f;

    [ExportGroup("Visual Properties")]

    [Export(PropertyHint.Range, "1,100,0.5")]
    public float LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            ApplyLineWidth();
        }
    }
    private float _lineWidth = 24f;

    [Export]
    public Curve WidthCurve
    {
        get => _widthCurve;
        set
        {
            _widthCurve = value;
            ApplyWidthCurve();
        }
    }
    private Curve _widthCurve;

    [ExportGroup("Shadow")]

    [Export]
    public Vector2 MinShadowOffset = new Vector2(0, 5);

    [Export]
    public Vector2 MaxShadowOffset = new Vector2(0, 20);

    #endregion

    #region Private Fields

    private readonly List<Vector2> _segments = new();
    private readonly List<float> _segmentLengths = new();
    private Vector2 _basePosition;
    private float _waveTime;

    #endregion

    public override void _Ready()
    {
        if (BaseNode != null)
            _basePosition = BaseNode.Position;

        InitializeSegments();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 targetPos = Target != null
            ? Target.GlobalPosition
            : GetGlobalMousePosition();

        SolveIk(targetPos);

        ApplyConstraints();
        ApplyWaveMotion((float)delta);
        ApplyConstraints();

        UpdateLine2D();
    }

    #region IK

    private void SolveIk(Vector2 targetPosition)
    {
        _segments[^1] = targetPosition;

        for (int iter = 0; iter < IkIterations; iter++)
        {
            // Backward pass
            for (int i = _numSegments - 1; i >= 0; i--)
            {
                Vector2 dir = (_segments[i] - _segments[i + 1]).Normalized();
                _segments[i] = _segments[i + 1] + dir * _segmentLengths[i];
            }

            // Forward pass
            _segments[0] = _basePosition;
            for (int i = 0; i < _numSegments; i++)
            {
                Vector2 dir = (_segments[i + 1] - _segments[i]).Normalized();
                _segments[i + 1] = _segments[i] + dir * _segmentLengths[i];
            }
        }
    }

    private void ApplyConstraints()
    {
        if (!EnableConstraint)
            return;

        _segments[0] = _basePosition;

        for (int iter = 0; iter < ConstraintIterations; iter++)
        {
            for (int i = 0; i < _numSegments; i++)
            {
                Vector2 vec = _segments[i + 1] - _segments[i];
                float dist = vec.Length();

                if (dist < 0.0001f)
                {
                    _segments[i + 1] = _segments[i] + Vector2.Right * _segmentLengths[i];
                    continue;
                }

                Vector2 targetVec = vec.Normalized() * _segmentLengths[i];
                Vector2 error = targetVec - vec;

                if (i > 0)
                    _segments[i] -= error * 0.25f;

                _segments[i + 1] += error * 0.25f;
            }

            _segments[0] = _basePosition;
        }
    }

    #endregion

    #region Wave Motion

    private void ApplyWaveMotion(float delta)
    {
        if (WaveAmplitude <= 0f)
            return;

        _waveTime += delta * WaveSpeed;

        float totalLength = 0f;
        foreach (float len in _segmentLengths)
            totalLength += len;

        float accumulated = 0f;

        for (int i = 1; i < _segments.Count; i++)
        {
            accumulated += _segmentLengths[i - 1];
            float t = accumulated / totalLength;

            Vector2 dir = (_segments[i] - _segments[i - 1]).Normalized();
            Vector2 perp = dir.Orthogonal();

            float phase = _waveTime + t * WaveFrequency * Mathf.Tau;
            float offset = Mathf.Sin(phase) * WaveAmplitude;

            _segments[i] += perp * offset;
        }
    }

    #endregion

    #region Rendering

    private void UpdateLine2D()
    {
        BaseNode?.ClearPoints();

        foreach (Vector2 pos in _segments)
            BaseNode?.AddPoint(BaseNode.ToLocal(pos));

        if (ShadowNode == null)
            return;

        ShadowNode.ClearPoints();

        for (int i = 0; i < _segments.Count; i++)
        {
            float t = (float)i / (_segments.Count - 1);
            Vector2 offset = MinShadowOffset.Lerp(MaxShadowOffset, t);
            Vector2 pos = _segments[i] + offset;

            ShadowNode.AddPoint(ShadowNode.ToLocal(pos));
        }
    }

    private void InitializeSegments()
    {
        if (BaseNode == null)
            return;

        _segments.Clear();
        _segmentLengths.Clear();

        _segments.Add(_basePosition);

        float length = _maxLength / _numSegments;

        for (int i = 0; i < _numSegments; i++)
        {
            _segmentLengths.Add(length);
            _segments.Add(_basePosition + new Vector2(length * (i + 1), 0));
        }

        UpdateLine2D();
    }

    private void ApplyLineWidth()
    {
        if (BaseNode != null)
            BaseNode.Width = _lineWidth;

        if (ShadowNode != null)
            ShadowNode.Width = _lineWidth;
    }

    private void ApplyWidthCurve()
    {
        if (_widthCurve == null)
            return;

        if (BaseNode != null)
            BaseNode.WidthCurve = _widthCurve;

        if (ShadowNode != null)
            ShadowNode.WidthCurve = _widthCurve;
    }

    #endregion

    #region Public Accessors

    public IReadOnlyList<Vector2> GetSegments() => _segments;
    public IReadOnlyList<float> GetSegmentLengths() => _segmentLengths;

    #endregion
}
