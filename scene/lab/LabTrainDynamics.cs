using Godot;
using System;

public partial class LabTrainDynamics : Node3D
{
    [Export] private Label _labelReverser;
    [Export] private Label _labelNotch;
    [Export] private Label _labelV;
    [Export] private Label _labelPR;
    [Export] private Label _labelCR;
    [Export] private Label _labelGR;

    [Export] private Node3D _train;
    
    // traction [N] of each notch
    private float[] _tractionArr =
    {
        -250000f, -194400f, -166600f, -138800f, -111200f, -83400f, -55600f, -27800f,
        0,
        41600f, 83400f, 125000f, 166600f
    };

    private const int NeutralNotch = 8;
    private int _notch = 0; // EB

    private float _v_kph = 0;
    private float _traction_N = 0;
    private Reverser _reverser = Reverser.Neutral;
    private TrackIterator _ti;
    private TrainDynamics _td;

    public override void _Ready()
    {
        FresnelIntegral.Approximate(20);

        RailProfile rp = new RailProfile(RailProfile.Axis.Z);
        rp.Load("res://resource/railProfile.obj");

        TrackPiece[] tpArr = new TrackPiece[4];

        tpArr[0] = new LineTrack().Define(20f, 0);
        tpArr[1] = new HorizontalCurveTrack().Define(258f, MathF.PI / 36, 0, 0);
        tpArr[2] = new HorizontalCurveTrack().Define(-258f, MathF.PI / 36, 0, 0);
        tpArr[3] = new LineTrack().Define(20f, 0);

        foreach(TrackPiece tp in tpArr)
        {
            tp.Bake().GenerateMesh(rp);
            AddChild(tp);
        }

        tpArr[1].Snap(Terminal.Initial, tpArr[0], Terminal.Final);
        tpArr[2].Snap(Terminal.Initial, tpArr[1], Terminal.Final);
        tpArr[3].Snap(Terminal.Initial, tpArr[2], Terminal.Final);

        tpArr[1].Link(Terminal.Initial, tpArr[0], Terminal.Final);
        tpArr[2].Link(Terminal.Initial, tpArr[1], Terminal.Final);
        tpArr[3].Link(Terminal.Initial, tpArr[2], Terminal.Final);

        tpArr[0].Link(Terminal.Initial, tpArr[3], Terminal.Final);

        StandardMaterial3D[] mArr = new StandardMaterial3D[tpArr.Length];
        for(int i=0; i<mArr.Length; i++)
        {
            mArr[i] = new StandardMaterial3D();

            if(i % 2 == 0) { mArr[i].AlbedoColor = Colors.Coral; }
            else { mArr[i].AlbedoColor = Colors.Orange; }

            tpArr[i].SetSurfaceOverrideMaterial(0, mArr[i]);
        }

        // rerail a train
        _ti = new TrackIterator(tpArr[0], 0, Terminal.Final);

        // train dynamics
        // model: KORAIL Class 351000 EMU
        _td = new TrainDynamics(1.435f, 2.1f, 200f, 78.48f, 1.867f, 0.0359f, 0.149f, 0.3f);

        // debug
        UpdateNotchLabel();
        UpdateReverserLabel();
    }

    public override void _Process(double delta)
    {
        Pose p = _ti.GetPose();

        // maximum speed
        float traction = _tractionArr[_notch];
        if(_v_kph >= 110f) { traction = 0; }

        float next_v = _td.UpdateVelocity(delta, p, _v_kph, _reverser, traction);
        _ti.Move((_v_kph + next_v) / 2 * (float)delta / 3.6f); // 3.6 km/h = 1 m/s
        _v_kph = next_v;

        if(_train != null)
        {
            p = _ti.GetPose();
            _train.Position = p.Position;
            _train.Basis = p.Axes;
        }

        // debug
        _labelV.Text = Math.Round(_v_kph, 1).ToString();
        _labelPR.Text = Math.Round(_td.CalculatePR(_v_kph)).ToString();
        _labelCR.Text = Math.Round(_td.CalculateCR(p.R)).ToString();
        _labelGR.Text = Math.Round(_td.CalculateGR(p.Gradient_permille)).ToString();
    }

    private void UpdateNotchLabel()
    {
        // update notch label
        if(_notch == 0)
        {
            _labelNotch.Text = "EB";
            _labelNotch.SelfModulate = Colors.Red;
        }
        else if(0 < _notch && _notch < NeutralNotch)
        {
            _labelNotch.Text = "B" + (NeutralNotch - _notch);
            _labelNotch.SelfModulate = Colors.Orange;
        }
        else if(_notch == NeutralNotch)
        {
            _labelNotch.Text = "N";
            _labelNotch.SelfModulate = Colors.Green;
        }
        else
        {
            _labelNotch.Text = "P" + (_notch - NeutralNotch);
            _labelNotch.SelfModulate = Colors.SkyBlue;
        }
    }

    private void UpdateReverserLabel()
    {
        // update reverser label
        _labelReverser.Text = _reverser.ToString();
    }

    // notch
    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton emb && emb.Pressed && !emb.IsEcho())
        {
            switch(emb.ButtonIndex)
            {
                case MouseButton.WheelUp:
                _notch--;
                if(_notch < 0) { _notch = 0; }
                break;

                case MouseButton.WheelDown:
                _notch++;
                if(_notch >= _tractionArr.Length) { _notch = _tractionArr.Length - 1; }
                break;
            }

            UpdateNotchLabel();
        }
    }

    // reverser
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if(@event is InputEventKey ek && ek.Pressed && !ek.Echo)
        {
            switch(ek.Keycode)
            {
                case Key.Up:
                _reverser = Reverser.Forward;
                break;

                case Key.Left:
                case Key.Right:
                _reverser = Reverser.Neutral;
                break;

                case Key.Down:
                _reverser = Reverser.Reverse;
                break;
            }

            UpdateReverserLabel();
        }
    }
}
