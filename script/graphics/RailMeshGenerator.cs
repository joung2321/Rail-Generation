using Godot;
using System;
using System.Collections.Generic;

public static class RailMeshGenerator
{
    public static ArrayMesh GenerateLine(RailProfile profile, float final_s_xz, float final_h, float ds = 0.5f)
    {
        float final_s = UnitSpeedCurve.LengthOfLine(final_s_xz, final_h);

        UnitSpeedCurve.Pose[] poses =
        {
            UnitSpeedCurve.Line(0, final_s_xz, final_h),
            UnitSpeedCurve.Line(final_s, final_s_xz, final_h)
        };

        return GenerateMesh(profile, poses);
    }

    public static ArrayMesh GenerateVerticalCurve(RailProfile profile, float r, float final_theta, float ds = 0.5f)
    {
        float final_s = UnitSpeedCurve.LengthOfVerticalCurve(r, final_theta);

        int poseCount = (int)Math.Ceiling(final_s / ds) + 1;
        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        for(int i=0; i<poseCount; i++)
        {
            float s = Math.Min(ds * i, final_s);
            poses[i] = UnitSpeedCurve.VerticalCurve(s, r);
        }

        return GenerateMesh(profile, poses);
    }

    public static ArrayMesh GenerateHorizontalCurve(RailProfile profile, float r, float final_theta, float bank, float pitch, float ds = 0.5f)
    {
        float final_s = UnitSpeedCurve.LengthOfHorizontalCurve(r, final_theta, pitch);

        int poseCount = (int)Math.Ceiling(final_s / ds) + 1;
        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        for(int i=0; i<poseCount; i++)
        {
            float s = Math.Min(ds * i, final_s);
            poses[i] = UnitSpeedCurve.HorizontalCurve(s, r, bank, pitch);
        }

        return GenerateMesh(profile, poses);
    }

    // A^2 = R * L
    public static ArrayMesh GenerateClothoid(RailProfile profile, float A, float final_r, float final_bank, float pitch, float ds = 0.5f)
    {
        float final_s = UnitSpeedCurve.LengthOfClothoid(A, final_r, pitch);
        
        int poseCount = (int)Math.Ceiling(final_s / ds) + 1;
        UnitSpeedCurve.Pose[] poses = new UnitSpeedCurve.Pose[poseCount];

        for(int i=0; i<poseCount; i++)
        {
            float s = Math.Min(ds * i, final_s);
            poses[i] = UnitSpeedCurve.Clothoid(s, A, final_r, final_bank, pitch);
        }

        return GenerateMesh(profile, poses);
    }

    public static ArrayMesh GenerateMesh(RailProfile profile, UnitSpeedCurve.Pose[] poses)
    {
        ReadOnlySpan<Vector3> profile_vArr = profile.Vertices;
        ReadOnlySpan<Vector2> profile_vtArr = profile.UVs;
        ReadOnlySpan<Vector3> profile_vnArr = profile.Normals;

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