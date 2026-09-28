using Godot;
using System;

public readonly record struct Pose
{
    public readonly Vector3 Position;
    public readonly Basis Axes;
    public readonly float Bank; // bank > 0: positive camber, bank < 0: negative camber
    public readonly float R; // curve radius r > 0
    
    public Pose(Vector3 position, Basis axes, float bank, float r = float.PositiveInfinity)
    {
        if(r < 0) { r = -r; }

        Position = position;
        Axes = axes;
        Bank = bank;
        R = r;
    }
    
    public float Gradient_permille => Axes.Column0.Y / MathF.Sqrt(Axes.Column0.X * Axes.Column0.X + Axes.Column0.Z * Axes.Column0.Z) * 1000;
};