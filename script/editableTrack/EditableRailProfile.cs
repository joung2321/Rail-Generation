using Godot;
using System;

[Tool]
public partial class EditableRailProfile : Node
{
    public event Action ProfileUpdated;
    public event Action ProfileDeleted;

    private RailProfile _profile; // null before _EnterTree() is called

    [ExportToolButton("Apply Changes")]
    private Callable _toolBtn_applyChanges => Callable.From(UpdateProfile);

    [Export] private RailProfile.Axis _lockedAxis = RailProfile.Axis.Z;
    [Export] private float _lockedValue = 0;
    [Export] private float _allowedError = 0;
    [Export] private ArrayMesh _mesh;
    [Export] private bool _isMonorail = false;
    [Export] private float _gauge_millimeter = 1435f;
    [Export] private float _samplingDistance_meter = 0.5f;
    [Export] private int _fresnelIntegralSize = 4;
    
    /// <summary>
    /// This property is null before _EnterTree() is called.
    /// </summary>
    public RailProfile Profile => _profile;
    public float SamplingDistance_m => _samplingDistance_meter;
    public float Gauge_mm => _gauge_millimeter;

    private void UpdateProfile()
    {
        if(_mesh != null)
        {
            FresnelIntegral.Approximate(_fresnelIntegralSize);

            _profile.LockedAxis = _lockedAxis;
            _profile.LockedValue = _lockedValue;
            _profile.AllowedError = _allowedError;

            _profile.ExtractProfileFrom(_mesh.ResourcePath);
        }
        else { _profile.Clear(); }
        
        ProfileUpdated?.Invoke();
    }

    public override void _EnterTree()
    {
        _profile = new RailProfile();
        UpdateProfile();
    }

    public override void _ExitTree()
    {
        ProfileDeleted?.Invoke();
    }
}
