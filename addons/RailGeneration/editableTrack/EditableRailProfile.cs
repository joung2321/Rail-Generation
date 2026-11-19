namespace RailGeneration;

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

    // mesh
    [Export] private float _targetZ = 0;
    [Export] private float _allowedError = 0;
    [Export] private ArrayMesh _mesh;

    // material
    [Export] private StandardMaterial3D _material;
    [Export] private RailProfile.UVAxis _flowAxis = RailProfile.UVAxis.None;
    [Export] private float _UVPerMeter = 1f;

    // procedural generation
    [Export] private bool _isMonorail = false;
    [Export] private float _gauge_millimeter = 1435f;
    [Export] private float _samplingDistance_meter = 0.5f;
    [Export] private int _fresnelIntegralSize = 4;

    public StandardMaterial3D Material => _material;
    public float Gauge_mm => _gauge_millimeter;
    public float SamplingDistance_m => _samplingDistance_meter;

    /// <summary>
    /// This property is null before _EnterTree() is called.
    /// </summary>
    public RailProfile Profile => _profile;

    private void UpdateProfile()
    {
        if(_mesh != null)
        {
            FresnelIntegral.Approximate(_fresnelIntegralSize);
            
            _profile.TargetZ = _targetZ;
            _profile.AllowedError = _allowedError;
            _profile.FlowAxis = _flowAxis;
            _profile.UVPerMeter = _UVPerMeter;
            _profile.Gauge_mm = _isMonorail? 0: _gauge_millimeter;

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
