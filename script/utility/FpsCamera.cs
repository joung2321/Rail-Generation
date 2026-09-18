using Godot;
using System;

// camera using pitch and yaw
public partial class FpsCamera : Camera3D
{
    [Export] private float _angleSpeed = 360;
    [Export] private float _linearSpeed = 4;
    
    private Vector2 _mouseRelative;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _mouseRelative = Vector2.Zero;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // rotation
        if(_mouseRelative != Vector2.Zero)
        {
            Vector2I screenSize = GetWindow().Size;

            // calculate pitch
            float pitch = Rotation.X - dt * _angleSpeed * _mouseRelative.Y / (MathF.PI * screenSize.Y);
            pitch = Math.Clamp(pitch, -MathF.PI / 2, MathF.PI / 2);

            // calculate yaw
            float yaw = Rotation.Y - dt * _angleSpeed * _mouseRelative.X / (MathF.PI * screenSize.X);

            // update rotation
            Rotation = new Vector3(pitch, yaw, 0);
            _mouseRelative = Vector2.Zero;
        }

        // position
        int front, right, up;
        front = right = up = 0;

        if(Input.IsKeyPressed(Key.W)) { front++; }
        if(Input.IsKeyPressed(Key.A)) { right--; }
        if(Input.IsKeyPressed(Key.S)) { front--; }
        if(Input.IsKeyPressed(Key.D)) { right++; }

        if(Input.IsKeyPressed(Key.Space)) { up++; }
        if(Input.IsKeyPressed(Key.Ctrl)) { up--; }

        Vector3 v = new Vector3(right, up, -front).Normalized();
        Position += dt * _linearSpeed * (GlobalTransform.Basis * v);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventMouseMotion emm)
        {
            _mouseRelative = emm.ScreenRelative;
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if(@event is InputEventKey ek && ek.Pressed && !ek.Echo)
        {
            switch(ek.Keycode)
            {
                case Key.Escape:
                Input.MouseMode = Input.MouseModeEnum.Visible;
                break;

                case Key.C:
                Input.MouseMode = Input.MouseModeEnum.Captured;
                break;
            }
        }
    }
}
