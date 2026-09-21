public partial class HorizontalCurveTrack : TrackPiece
{
    private float _r;
    private float _final_theta;
    private float _bank;
    private float _pitch;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float r, float final_theta, float bank, float pitch)
    {
        _r = r;
        _final_theta = final_theta;
        _bank = bank;
        _pitch = pitch;

        return this;
    }

    public override UnitSpeedCurve.Pose C(float s)
    {
        return UnitSpeedCurve.HorizontalCurve(s, _r, _bank, _pitch);
    }

    protected override float CalculateTotalLength()
    {
        return UnitSpeedCurve.LengthOfHorizontalCurve(_r, _final_theta, _pitch);
    }
}
