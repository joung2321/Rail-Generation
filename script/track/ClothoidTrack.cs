public partial class ClothoidTrack : TrackPiece
{
    private float _final_r;
    private float _A;
    private float _pitch;
    private float _final_bank;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float final_r, float A, float pitch, float final_bank)
    {
        _final_r = final_r;
        _A = A;
        _pitch = pitch;
        _final_bank = final_bank;
        
        return this;
    }

    public override Pose C(float s)
    {
        return UnitSpeedCurve.Clothoid(s, _final_r, _A, _pitch, _final_bank);
    }

    protected override float CalculateTotalLength()
    {
        return UnitSpeedCurve.LengthOfClothoid(_final_r, _A, _pitch);
    }
}
