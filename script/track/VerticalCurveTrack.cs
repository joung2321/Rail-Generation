public partial class VerticalCurveTrack : TrackPiece
{
    private float _r;
    private float _final_theta;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float r, float final_theta)
    {
        _r = r;
        _final_theta = final_theta;

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
