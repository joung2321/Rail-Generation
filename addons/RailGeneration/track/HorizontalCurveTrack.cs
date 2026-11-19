namespace RailGeneration;

using Godot;
using System;
using RailGeneration.Common;

public partial class HorizontalCurveTrack : TrackPiece
{
    private float _r;
    private float _final_theta;
    private float _pitch;
    private float _bank;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float r_m, float theta_deg, float gradient_permille, float cant_mm, float gauge_mm = 1435f)
    {
        _r = r_m;
        _final_theta = Mathf.DegToRad(theta_deg);
        _pitch = MathF.Atan(gradient_permille / 1000);
        _bank = MathF.Asin(cant_mm / gauge_mm);
        
        return this;
    }

    public override Pose C(float s)
    {
        return UnitSpeedCurve.HorizontalCurve(s, _r, _pitch, _bank);
    }

    protected override float CalculateTotalLength()
    {
        return UnitSpeedCurve.LengthOfHorizontalCurve(_r, _final_theta, _pitch);
    }
}
