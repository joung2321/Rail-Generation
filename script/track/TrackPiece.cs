using Godot;
using System;

/// <typeparam name="TCurveShape">a set of parameters describing a curve</typeparam>
public abstract partial class TrackPiece : MeshInstance3D
{
    public float TotalLength { get; private set; }
    public UnitSpeedCurve.Pose InitialPose { get; private set; } // C(0)
    public UnitSpeedCurve.Pose FinalPose { get; private set; } // C(TotalLength)

    /// <summary>
    /// TODO: calculate and return TotalLength using this.CurveShape
    /// </summary>
    /// <param name="curveShape">a set of parameters describing this curve</param>
    protected abstract float CalculateTotalLength();

    /// <summary>
    /// a unit speed curve
    /// </summary>
    /// <param name="s">arc length</param>
    public abstract UnitSpeedCurve.Pose C(float s);

    /// <summary>
    /// pre-calculates TotalLength, InitialPose, FinalPose
    /// </summary>
    /// <returns>returns itself</returns>
    public TrackPiece Bake()
    {
        TotalLength = CalculateTotalLength();
        InitialPose = C(0);
        FinalPose = C(TotalLength);

        return this;
    }

    public virtual void GenerateMesh(RailProfile profile, float ds = 0.5f)
    {
        int poseCount = (int)Math.Ceiling(TotalLength / ds) + 1;
        if(poseCount < 2) { return; }

        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        // fill array
        poses[0] = InitialPose;

        for(int i=1; i<poseCount; i++)
        {
            float s = ds * i;

            if(s < TotalLength) { poses[i] = C(s); }
            else { poses[i] = FinalPose; }
        }

        Mesh = RailMeshGenerator.GenerateMesh(profile, poses);
    }
}
