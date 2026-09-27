using Godot;
using System;

public partial class LabTrackConnector : Node3D
{
    [Export] private MeshInstance3D _trainMesh;
    [Export] private float _speed = 0;
    BaseTrain _bt;

    public override void _Ready()
    {
        FresnelIntegral.Approximate(20);

        RailProfile rp = new RailProfile(RailProfile.Axis.Z);
        rp.Load("res://resource/railProfile.obj");

        TrackPiece[] tpArr = new TrackPiece[4];

        tpArr[0] = new LineTrack().Define(4f, 0f);
        tpArr[1] = new HorizontalCurveTrack().Define(-2f, MathF.PI, 0, 0);
        tpArr[2] = new LineTrack().Define(4f, 0f);
        tpArr[3] = new HorizontalCurveTrack().Define(-2f, MathF.PI, 0, 0);
        
        foreach(TrackPiece tp in tpArr)
        {
            tp.Bake().GenerateMesh(rp);
            AddChild(tp);
        }

        tpArr[0].Position = Vector3.One;
        tpArr[0].RotateY(MathF.PI / 4);

        tpArr[1].Snap(Terminal.Initial, tpArr[0], Terminal.Final);
        tpArr[2].Snap(Terminal.Initial, tpArr[1], Terminal.Final);
        tpArr[3].Snap(Terminal.Initial, tpArr[2], Terminal.Final);

        tpArr[1].Link(Terminal.Initial, tpArr[0], Terminal.Final);
        tpArr[2].Link(Terminal.Initial, tpArr[1], Terminal.Final);
        tpArr[3].Link(Terminal.Initial, tpArr[2], Terminal.Final);
        tpArr[0].Link(Terminal.Initial, tpArr[3], Terminal.Final);
        
        StandardMaterial3D[] mArr = new StandardMaterial3D[4];
        for(int i=0; i<mArr.Length; i++) { mArr[i] = new StandardMaterial3D(); }

        mArr[0].AlbedoColor = Colors.Red;
        mArr[1].AlbedoColor = Colors.Orange;
        mArr[2].AlbedoColor = Colors.Yellow;
        mArr[3].AlbedoColor = Colors.Green;

        for(int i=0; i<mArr.Length; i++)
        {
            tpArr[i].SetSurfaceOverrideMaterial(0, mArr[i]);
        }

        // rerail a train
        _bt = new BaseTrain(tpArr[0], 0, Terminal.Final);
    }

    public override void _Process(double delta)
    {
        _bt.Move(_speed * (float)delta);

        if(_trainMesh != null)
        {
            Pose p = _bt.GetPose();
            _trainMesh.Position = p.Position;
            _trainMesh.Basis = p.Axes;
        }
    }

}
