using Godot;

public partial class LineTrack : TrackPiece
{
    private float _final_s_xz;
    private float _final_h;

    /// <returns>returns itself</returns>
    public TrackPiece Define(float final_s_xz, float final_h)
    {
        _final_s_xz = final_s_xz;
        _final_h = final_h;

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
