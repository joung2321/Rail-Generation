using Godot;

public partial class LineTrack : TrackPiece
{
    private float _final_s_xz;
    private float _final_h;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float horizontalLength_m, float gradient_permille)
    {
        _final_s_xz = horizontalLength_m;
        _final_h = horizontalLength_m * gradient_permille / 1000;

        return this;
    }

    public override Pose C(float s)
    {
        return UnitSpeedCurve.Line(s, _final_s_xz, _final_h);
    }

    protected override float CalculateTotalLength()
    {
        return UnitSpeedCurve.LengthOfLine(_final_s_xz, _final_h);
    }

    public override void GenerateMesh(RailProfile profile, float ds)
    {
        Pose[] poses = { InitialPose, FinalPose };
        Mesh = RailMeshGenerator.GenerateMesh(profile, poses);
    }
}
