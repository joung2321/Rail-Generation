public partial class ClothoidTrack : TrackPiece
{
    private float _A;
    private float _final_r;
    private float _final_bank;
    private float _pitch;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float A, float final_r, float final_bank, float pitch)
    {
        _A = A;
        _final_r = final_r;
        _final_bank = final_bank;
        _pitch = pitch;

        return this;
    }

    public override UnitSpeedCurve.Pose C(float s)
    {
        return UnitSpeedCurve.Clothoid(s, _A, _final_r, _final_bank, _pitch);
    }

    protected override float CalculateTotalLength()
    {
        return UnitSpeedCurve.LengthOfClothoid(_A, _final_r, _pitch);
    }
}
