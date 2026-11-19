namespace RailGeneration;

using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// a class extracting cross section from mesh<br/>
/// cutting plane: z = RailProfile.TargetZ
/// </summary>
public class RailProfile
{
    public enum UVAxis { None, U, V };

    public float TargetZ = 0;
    private float _allowedError = 0;
    private float _gauge_mm = 0;
    
    public UVAxis FlowAxis = UVAxis.None;
    public float UVPerMeter = 1f;

    // ensure _allowedError >= 0
    public float AllowedError
    {
        get { return _allowedError; }
        set { _allowedError = (value < 0)? -value: value; }
    }

    // ignore _gauge_mm < 0
    public float Gauge_mm
    {
        get { return _gauge_mm; }
        set { if(value >= 0) { _gauge_mm = value; } }
    }

    // _vArr[2k] and _vArr[2k+1] define a edge in cross section.
    // _vtArr[i] and _vnArr[i] describe a vertex _vArr[i].
    // however, _vArr[2k+1] and _vArr[2k+2] have no relation.
    private Vector3[] _vArr;
    private Vector2[] _vtArr;
    private Vector3[] _vnArr;

    public ReadOnlySpan<Vector3> Vertices => _vArr;
    public ReadOnlySpan<Vector2> UVs => _vtArr;
    public ReadOnlySpan<Vector3> Normals => _vnArr;

    private bool CheckConstraint(Vector3 v)
    {
        return MathF.Abs(v.Z - TargetZ) <= _allowedError;
    }

    public void Clear()
    {
        _vArr = null;
        _vtArr = null;
        _vnArr = null;
    }
    
    public void ExtractProfileFrom(string filePath)
    {
        ArrayMesh am = ResourceLoader.Load<ArrayMesh>(filePath);
        MeshDataTool mdt = new MeshDataTool();

        mdt.CreateFromSurface(am, 0);

        int listSize = 2 * mdt.GetEdgeCount();
        if(_gauge_mm > 0) { listSize *= 2; }

        List<Vector3> vList = new List<Vector3>(listSize);
        List<Vector2> vtList = new List<Vector2>(listSize);
        List<Vector3> vnList = new List<Vector3>(listSize);
        
        // find edges that belongs to only 1 triangle
        for(int e=0; e<mdt.GetEdgeCount(); e++)
        {
            int[] f = mdt.GetEdgeFaces(e); // faces that contains edge e
            if(f.Length == 1)
            {
                int[] fvArr = new int[3];
                bool[] isValid = new bool[3];
                Vector3[] faceVertexArr = new Vector3[3];

                // check vertices in CCW order
                for(int v=0; v<3; v++)
                {
                    fvArr[v] = mdt.GetFaceVertex(f[0], v);
                    faceVertexArr[v] = mdt.GetVertex(fvArr[v]);
                    isValid[v] = CheckConstraint(faceVertexArr[v]);
                }

                // add valid edge in CCW order
                for(int i=0; i<3; i++)
                {
                    int fv0 = i % 3;
                    int fv1 = (i + 1) % 3;

                    if(isValid[fv0] && isValid[fv1])
                    {
                        Vector3 vertex;

                        // fv0
                        vertex = faceVertexArr[fv0];
                        vertex.Z = 0;

                        vList.Add(vertex);
                        vtList.Add(mdt.GetVertexUV(fvArr[fv0]));
                        vnList.Add(mdt.GetVertexNormal(fvArr[fv0]));

                        // fv1
                        vertex = faceVertexArr[fv1];
                        vertex.Z = 0;

                        vList.Add(vertex);
                        vtList.Add(mdt.GetVertexUV(fvArr[fv1]));
                        vnList.Add(mdt.GetVertexNormal(fvArr[fv1]));
                    }
                }
            }
        }

        // duplicate profile
        if(_gauge_mm > 0)
        {
            float halfGauge_m = _gauge_mm / (2 * 1000);

            vList.AddRange(vList);
            vtList.AddRange(vtList);
            vnList.AddRange(vnList);

            // list size
            int full = vList.Count;
            int half = full / 2;

            for(int i=0; i<half; i++) { vList[i] += halfGauge_m * Vector3.Right; }
            for(int i=half; i<full; i++) { vList[i] += halfGauge_m * Vector3.Left; }
        }

        // convert list to array
        _vArr = vList.ToArray();
        _vtArr = vtList.ToArray();
        _vnArr = vnList.ToArray();
    }
}
