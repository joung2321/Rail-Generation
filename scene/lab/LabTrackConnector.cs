using Godot;
using System;

public partial class LabTrackConnector : Node3D
{
    public override void _Ready()
    {
        FresnelIntegral.Approximate(20);

        RailProfile rp = new RailProfile(RailProfile.Axis.Z);
        rp.Load("res://resource/railProfile.obj");

        TrackPiece[] tpArr = new TrackPiece[5];
        tpArr[0] = new LineTrack().Define(2f, 0f);
        tpArr[1] = new ClothoidTrack().Define(6f, 8f, MathF.PI / 12, 0);
        tpArr[2] = new HorizontalCurveTrack().Define(-8f, MathF.PI / 4, MathF.PI / 12, 0);
        tpArr[3] = new ClothoidTrack().Define(6f, -8f, MathF.PI / 12, 0);
        tpArr[4] = new LineTrack().Define(2f, 0f);
        
        foreach(TrackPiece tp in tpArr)
        {
            tp.Bake().GenerateMesh(rp);
            AddChild(tp);
        }

        tpArr[0].Position = Vector3.One;
        tpArr[0].RotateY(MathF.PI / 4);

        tpArr[1].Snap(Terminal.Initial, tpArr[0], Terminal.Final);
        tpArr[2].Snap(Terminal.Final, tpArr[1], Terminal.Final);
        tpArr[3].Snap(Terminal.Final, tpArr[2], Terminal.Initial);
        tpArr[4].Snap(Terminal.Initial, tpArr[3], Terminal.Initial);
        
        StandardMaterial3D[] mArr = new StandardMaterial3D[5];
        for(int i=0; i<mArr.Length; i++) { mArr[i] = new StandardMaterial3D(); }

        mArr[0].AlbedoColor = Colors.Red;
        mArr[1].AlbedoColor = Colors.Orange;
        mArr[2].AlbedoColor = Colors.Yellow;
        mArr[3].AlbedoColor = Colors.Green;
        mArr[4].AlbedoColor = Colors.Blue;

        for(int i=0; i<mArr.Length; i++)
        {
            tpArr[i].SetSurfaceOverrideMaterial(0, mArr[i]);
        }
    }
}
