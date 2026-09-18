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

        RailGenerator rg = new RailGenerator(rp, 0.5f);

        _miArr = new MeshInstance3D[5]; // index: 1~4

        for(int i=1; i<_miArr.Length; i++)
        {
            _miArr[i] = new MeshInstance3D();
            _miArr[i].Visible = i == 1;

            AddChild(_miArr[i]);
        }

        _miArr[1].Mesh = rg.GenerateLine(4f, 2f);
        _miArr[2].Mesh = rg.GenerateVerticalCurve(4f, MathF.PI / 3);
        _miArr[3].Mesh = rg.GenerateHorizontalCurve(3f, 4 * MathF.PI, MathF.PI / 8, MathF.PI / 32);
        _miArr[4].Mesh = rg.GenerateClothoid(8f, 2f, MathF.PI / 8, MathF.PI / 32);
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