using System;
using Godot;

public partial class LabRailProfile : Node3D
{
    MeshInstance3D[] _miArr;

    public override void _Ready()
    {
        FresnelIntegral.Approximate(20);

        RailProfile rp = new RailProfile(RailProfile.Axis.Z);
        rp.Load("res://resource/railProfile.obj");

        _miArr = new TrackPiece[1+4]; // index: 1~4

        _miArr[1] = new LineTrack().Define(4f, 2f).Bake().GenerateMesh(rp);
        _miArr[2] = new VerticalCurveTrack().Define(4f, MathF.PI / 3).Bake().GenerateMesh(rp);
        _miArr[3] = new HorizontalCurveTrack().Define(3f, 4 * MathF.PI, MathF.PI / 8, MathF.PI / 32).Bake().GenerateMesh(rp);
        _miArr[4] = new ClothoidTrack().Define(8f, 2f, MathF.PI / 8, MathF.PI / 32).Bake().GenerateMesh(rp);

        for(int i=1; i<_miArr.Length; i++)
        {
            _miArr[i].Visible = i == 1;
            AddChild(_miArr[i]);
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if(@event is InputEventKey ek && ek.Pressed && !ek.Echo)
        {
            switch(ek.Keycode)
            {
                case Key.Key1:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 1; }
                break;

                case Key.Key2:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 2; }
                break;

                case Key.Key3:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 3; }
                break;

                case Key.Key4:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 4; }
                break;
            }
        }
    }
}