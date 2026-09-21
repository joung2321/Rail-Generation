using Godot;
using System;
using System.Collections.Generic;

public static class RailMeshGenerator
{
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