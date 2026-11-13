using System;
using Godot;
using Godot.Collections;

[Tool]
public partial class EditableTrackPiece : TrackPiece
{
    public enum TrackType { Line, VerticalCurve, HorizontalCurve, Clothoid };

    // parameters describing curve
    private float[] _params = new float[4];

    // index for _params
    private enum Param
    {
        None = -1,
        Sxz = 0, R = 0,
        Theta = 1, L = 1,
        Gradient = 2,
        Cant = 3
    };

    // names for parameters
    private const string NameSxz        = "horizontal length [m]";
    private const string NameR          = "r [m]";
    private const string NameTheta      = "theta [deg]";
    private const string NameL          = "L [m]";
    private const string NameGradient   = "gradient [permille]";
    private const string NameCant       = "cant [mm]";

    // baked parameters
    private float _bakedHeight;
    private float _bakedTheta_rad;
    private float _bakedA_m;
    private float _bakedPitch_rad;
    private float _bakedBank_rad;

    private EditableRailProfile _editableProfile;
    private TrackType _type;
    private Mesh _generatedMesh;

    [ExportToolButton("Print Yaw [deg]")]
    private Callable _toolBtn_printYaw => Callable.From(PrintYaw);

    [ExportToolButton("Snap Initial Terminal to Prev")]
    private Callable _toolBtn_snapToPrev => Callable.From(SnapToPrev);
    [Export] private TrackPiece _prevPiece;
    [Export] private Terminal _prevTerminal;

    [ExportToolButton("Snap Final Terminal to Next")]
    private Callable _toolBtn_snapToNext => Callable.From(SnapToNext);
    [Export] private TrackPiece _nextPiece;
    [Export] private Terminal _nextTerminal;

    [ExportToolButton("Regenerate Mesh")]
    private Callable _toolBtn_regenerateMesh => Callable.From(RegenerateMesh);

    [Export] private EditableRailProfile EditableProfile
    {
        get { return _editableProfile; }
        set
        {
            UnsubscribeProfile();

            _editableProfile = value;

            // subscribe events
            if(_editableProfile != null)
            {
                _editableProfile.ProfileUpdated += RegenerateMesh;
                _editableProfile.ProfileDeleted += UnsubscribeProfile;
            }
        }
    }
    
    [Export] private TrackType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            NotifyPropertyListChanged();
        }
    }

    private void PrintYaw()
    {
        GD.Print($"# Yaw of {Name}\n- local : {RotationDegrees.Y}\n- global: {GlobalRotationDegrees.Y}");
    }

    private void SnapToPrev()
    {
        if(_prevPiece != null) { Snap(Terminal.Initial, _prevPiece, _prevTerminal); }
    }

    private void SnapToNext()
    {
        if(_nextPiece != null) { Snap(Terminal.Final, _nextPiece, _nextTerminal); }
    }

    private void RegenerateMesh()
    {
        if(_editableProfile == null || _editableProfile.Profile == null) { return; }

        Bake(); // pre-calculates TotalLength, InitialPose, FinalPose
        GenerateMesh(_editableProfile.Profile, _editableProfile.SamplingDistance_m);
    }
    
    private void UnsubscribeProfile()
    {
        // unsubscribe events
        if(_editableProfile != null)
        {
            _editableProfile.ProfileUpdated -= RegenerateMesh;
            _editableProfile.ProfileDeleted -= UnsubscribeProfile;
        }

        _editableProfile = null;
    }

    private void AddProperty(Array<Dictionary> properties, string name)
    {
        properties.Add(new Dictionary
        {
            { "name", name },
            { "type", (int)Variant.Type.Float },
            { "usage", (int)PropertyUsageFlags.Default }
        });
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        Array<Dictionary> properties = new Array<Dictionary>();
        
        switch(_type)
        {
            case TrackType.Line:
            AddProperty(properties, NameSxz);
            AddProperty(properties, NameGradient);
            break;

            case TrackType.VerticalCurve:
            AddProperty(properties, NameR);
            AddProperty(properties, NameGradient);
            break;

            case TrackType.HorizontalCurve:
            AddProperty(properties, NameR);
            AddProperty(properties, NameTheta);
            AddProperty(properties, NameGradient);
            AddProperty(properties, NameCant);
            break;

            case TrackType.Clothoid:
            AddProperty(properties, NameR);
            AddProperty(properties, NameL);
            AddProperty(properties, NameGradient);
            AddProperty(properties, NameCant);
            break;
        }

        return properties;
    }

    private static int StringNameToIndex(StringName property)
    {
        Param p = Param.None;

        switch(property)
        {
            case NameSxz:       p = Param.Sxz;
            break;
            case NameR:         p = Param.R;
            break;
            case NameTheta:     p = Param.Theta;
            break;
            case NameL:         p = Param.L;
            break;
            case NameGradient:  p = Param.Gradient;
            break;
            case NameCant:      p = Param.Cant;
            break;
        }

        return (int)p;
    }

    public override Variant _Get(StringName property)
    {
        int idx = StringNameToIndex(property);

        if(idx < 0) { return base._Get(property); }
        else { return _params[idx]; }
    }

    public override bool _Set(StringName property, Variant value)
    {
        int idx = StringNameToIndex(property);

        if(idx < 0) { return base._Set(property, value); }
        else
        {
            _params[idx] = (float)value;
            return true;
        }
    }

    // [IMPORTANT] prevent this.Mesh from being saved to .tscn
    public override void _Notification(int what)
    {
        switch((long)what)
        {
            case NotificationEditorPreSave:
            _generatedMesh = Mesh;
            Mesh = null;
            break;

            case NotificationEditorPostSave:
            Mesh = _generatedMesh;
            _generatedMesh = null;
            break;
        }
    }

    public override void _EnterTree()
    {
        // prevent redundant mesh generation
        if(Mesh == null) { RegenerateMesh(); }

        if(!Engine.IsEditorHint())
        {
            Link(Terminal.Initial, _prevPiece, _prevTerminal);
            Link(Terminal.Final, _nextPiece, _nextTerminal);
        }
    }
    
    public override void _ExitTree()
    {
        UnsubscribeProfile();
    }

    protected override void _Bake()
    {
        switch(_type)
        {
            case TrackType.Line:
            _bakedHeight = _params[(int)Param.Sxz] * _params[(int)Param.Gradient] / 1000;
            return;

            case TrackType.VerticalCurve:
            _bakedTheta_rad = MathF.Atan(_params[(int)Param.Gradient] / 1000);
            if(_bakedTheta_rad < 0) { _bakedTheta_rad = -_bakedTheta_rad; }
            return;

            case TrackType.HorizontalCurve:
            _bakedTheta_rad = Mathf.DegToRad(_params[(int)Param.Theta]);
            break;

            case TrackType.Clothoid:
            float A2 = _params[(int)Param.R] * _params[(int)Param.L]; // A^2
            if(A2 < 0) { A2 = -A2; }
            _bakedA_m = MathF.Sqrt(A2);
            break;
        }

        _bakedPitch_rad = MathF.Atan(_params[(int)Param.Gradient] / 1000);
        _bakedBank_rad = MathF.Asin(_params[(int)Param.Cant] / _editableProfile.Gauge_mm);
    }

    protected override float CalculateTotalLength()
    {
        switch(_type)
        {
            case TrackType.Line:
            return UnitSpeedCurve.LengthOfLine(_params[(int)Param.Sxz], _bakedHeight);

            case TrackType.VerticalCurve:
            return UnitSpeedCurve.LengthOfVerticalCurve(_params[(int)Param.R], _bakedTheta_rad);

            case TrackType.HorizontalCurve:
            return UnitSpeedCurve.LengthOfHorizontalCurve(_params[(int)Param.R], _bakedTheta_rad, _bakedPitch_rad);

            case TrackType.Clothoid:
            return UnitSpeedCurve.LengthOfClothoid(_params[(int)Param.R], _bakedA_m, _bakedPitch_rad);

            default:
            return 0;
        }
    }

    public override Pose C(float s)
    {
        switch(_type)
        {
            case TrackType.Line:
            return UnitSpeedCurve.Line(s, _params[(int)Param.Sxz], _bakedHeight);

            case TrackType.VerticalCurve:
            return UnitSpeedCurve.VerticalCurve(s, _params[(int)Param.R]);

            case TrackType.HorizontalCurve:
            return UnitSpeedCurve.HorizontalCurve(s, _params[(int)Param.R], _bakedPitch_rad, _bakedBank_rad);

            case TrackType.Clothoid:
            return UnitSpeedCurve.Clothoid(s, _params[(int)Param.R], _bakedA_m, _bakedPitch_rad, _bakedBank_rad);

            default:
            return new Pose();
        }
    }

    public override void GenerateMesh(RailProfile profile, float ds = 0.5f)
    {
        if(_type == TrackType.Line)
        {
            Pose[] poses = { InitialPose, FinalPose };
            Mesh = RailMeshGenerator.GenerateMesh(_editableProfile.Profile, poses);
        }
        else
        {
            base.GenerateMesh(_editableProfile.Profile, _editableProfile.SamplingDistance_m);
        }
    }
}
