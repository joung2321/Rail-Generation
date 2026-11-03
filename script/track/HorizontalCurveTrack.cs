public partial class HorizontalCurveTrack : TrackPiece
{
    private float _r;
    private float _final_theta;
    private float _pitch;
    private float _bank;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float r, float final_theta, float pitch, float bank)
    {
        _r = r;
        _final_theta = final_theta;
        _pitch = pitch;
        _bank = bank;

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
