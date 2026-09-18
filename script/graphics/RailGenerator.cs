using Godot;
using System;
using System.Collections.Generic;

public class RailGenerator
{
    private RailProfile _rp;
    private float _ds;

    public RailGenerator(RailProfile railProfile, float ds)
    {
        _rp = railProfile;
        _ds = ds;
    }

    public ArrayMesh GenerateLine(float final_s_xz, float final_h)
    {
        float final_s = MathF.Sqrt(final_s_xz * final_s_xz + final_h * final_h);

        UnitSpeedCurve.Pose[] poses =
        {
            UnitSpeedCurve.Line(0, final_s_xz, final_h),
            UnitSpeedCurve.Line(final_s, final_s_xz, final_h)
        };

        return GenerateMesh(poses);
    }

    public ArrayMesh GenerateVerticalCurve(float r, float final_theta)
    {
        float final_s = r * final_theta;

        int poseCount = (int)Math.Ceiling(final_s / _ds) + 1;
        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        for(int i=0; i<poseCount; i++)
        {
            float s = Math.Min(_ds * i, final_s);
            poses[i] = UnitSpeedCurve.VerticalCurve(s, r);
        }

        return GenerateMesh(poses);
    }

    public ArrayMesh GenerateHorizontalCurve(float r, float final_theta, float roll, float pitch)
    {
        float final_s = r * final_theta / MathF.Cos(pitch);

        int poseCount = (int)Math.Ceiling(final_s / _ds) + 1;
        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        for(int i=0; i<poseCount; i++)
        {
            float s = Math.Min(_ds * i, final_s);
            poses[i] = UnitSpeedCurve.HorizontalCurve(s, r, roll, pitch);
        }

        return GenerateMesh(poses);
    }

    // A^2 = R * L
    public ArrayMesh GenerateClothoid(float A, float final_r, float final_roll, float pitch)
    {
        float final_s = A * A / final_r;

        int poseCount = (int)Math.Ceiling(final_s / _ds) + 1;
        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        for(int i=0; i<poseCount; i++)
        {
            float s = Math.Min(_ds * i, final_s);
            poses[i] = UnitSpeedCurve.Clothoid(s, A, final_r, final_roll, pitch);
        }

        return GenerateMesh(poses);
    }

    private ArrayMesh GenerateMesh(UnitSpeedCurve.Pose[] poses)
    {
        ReadOnlySpan<Vector3> profile_vArr = _rp.Vertices;
        ReadOnlySpan<Vector2> profile_vtArr = _rp.UVs;
        ReadOnlySpan<Vector3> profile_vnArr = _rp.Normals;

        if(profile_vArr.Length == 0 || poses.Length < 2) { return null; }
        
        int vCount = poses.Length * profile_vArr.Length;

        List<Vector3> mesh_vList = new List<Vector3>(vCount);
        List<Vector2> mesh_vtList = new List<Vector2>(vCount);
        List<Vector3> mesh_vnList = new List<Vector3>(vCount);
        List<int> mesh_idxList = new List<int>(6 * (poses.Length - 1) * (profile_vArr.Length / 2));

        for(int i=0; i<poses.Length; i++)
        {
            // if let v[i] = profile_vArr[i],
            // then v[2k] and v[2k+1] define a edge.
            // however, v[2k+1] and v[2k+2] have no relation.
            for(int j=0; j<profile_vArr.Length; j++)
            {
                mesh_vList.Add(poses[i].Position + poses[i].Axes * profile_vArr[j]);
                mesh_vtList.Add(profile_vtArr[j]);
                mesh_vnList.Add(poses[i].Axes * profile_vnArr[j]);

                // indexing
                if(i < poses.Length - 1 && j % 2 == 0)
                {
                    int currIdx = profile_vArr.Length * i + j;
                    int nextIdx = profile_vArr.Length * (i + 1) + j;

                    // 1st triangle
                    mesh_idxList.Add(currIdx);
                    mesh_idxList.Add(currIdx + 1);
                    mesh_idxList.Add(nextIdx);

                    // 2nd triangle
                    mesh_idxList.Add(currIdx + 1);
                    mesh_idxList.Add(nextIdx + 1);
                    mesh_idxList.Add(nextIdx);
                }
            }
        }

        // create ArrayMesh
        Godot.Collections.Array surface = [];
        surface.Resize((int)Mesh.ArrayType.Max);

        surface[(int)Mesh.ArrayType.Vertex] = mesh_vList.ToArray();
        surface[(int)Mesh.ArrayType.TexUV] = mesh_vtList.ToArray();
        surface[(int)Mesh.ArrayType.Normal] = mesh_vnList.ToArray();
        surface[(int)Mesh.ArrayType.Index] = mesh_idxList.ToArray();

        ArrayMesh mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surface);

        return mesh;
    }
}