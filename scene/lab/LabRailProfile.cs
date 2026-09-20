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

        _miArr = new MeshInstance3D[9]; // index: 1~8

        for(int i=1; i<_miArr.Length; i++)
        {
            _miArr[i] = new MeshInstance3D();
            _miArr[i].Visible = i == 1;

            AddChild(_miArr[i]);
        }

        // r > 0, h > 0
        _miArr[1].Mesh = RailMeshGenerator.GenerateLine(rp, 4f, 2f);
        _miArr[2].Mesh = RailMeshGenerator.GenerateVerticalCurve(rp, 4f, MathF.PI / 3);
        _miArr[3].Mesh = RailMeshGenerator.GenerateHorizontalCurve(rp, 3f, 4 * MathF.PI, MathF.PI / 8, MathF.PI / 32);
        _miArr[4].Mesh = RailMeshGenerator.GenerateClothoid(rp, 8f, 2f, MathF.PI / 8, MathF.PI / 32);

        // r < 0, h < 0
        _miArr[5].Mesh = RailMeshGenerator.GenerateLine(rp, 4f, -2f);
        _miArr[6].Mesh = RailMeshGenerator.GenerateVerticalCurve(rp, -4f, MathF.PI / 3);
        _miArr[7].Mesh = RailMeshGenerator.GenerateHorizontalCurve(rp, -3f, 4 * MathF.PI, MathF.PI / 8, -MathF.PI / 32);
        _miArr[8].Mesh = RailMeshGenerator.GenerateClothoid(rp, 8f, -2f, MathF.PI / 8, -MathF.PI / 32);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if(@event is InputEventKey ek && ek.Pressed && !ek.Echo)
        {
            switch(ek.Keycode)
            {
                // r > 0, h > 0
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

                // r < 0, h < 0
                case Key.Key5:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 5; }
                break;

                case Key.Key6:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 6; }
                break;

                case Key.Key7:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 7; }
                break;

                case Key.Key8:
                for(int i=1; i<_miArr.Length; i++) { _miArr[i].Visible = i == 8; }
                break;
            }
        }
    }
}