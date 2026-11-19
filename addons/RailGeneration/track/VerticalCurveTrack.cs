namespace RailGeneration;

using System;
using RailGeneration.Common;

public partial class VerticalCurveTrack : TrackPiece
{
    private float _r;
    private float _final_theta;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float r_m, float gradient_permille)
    {
        _r = r_m;
        _final_theta = MathF.Atan(gradient_permille / 1000);
        if(_final_theta < 0) { _final_theta = -_final_theta; }

        return this;
    }
    
    public override Pose C(float s)
    {
        return UnitSpeedCurve.VerticalCurve(s, _r);
    }

    protected override float CalculateTotalLength()
    {
        return UnitSpeedCurve.LengthOfVerticalCurve(_r, _final_theta);
    }
}
