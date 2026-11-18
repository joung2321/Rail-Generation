using System;

public partial class ClothoidTrack : TrackPiece
{
    private float _final_r;
    private float _A;
    private float _pitch;
    private float _final_bank;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float R_m, float L_m, float gradient_permille, float cant_mm, float gauge_mm = 1435f)
    {
        _final_r = R_m;
        _pitch = MathF.Atan(gradient_permille / 1000);
        _final_bank = MathF.Asin(cant_mm / gauge_mm);

        // A^2 = R * L
        float A2 = R_m * L_m; // A^2
        if(A2 < 0) { A2 = -A2; }
        _A = MathF.Sqrt(A2);

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
