using Godot;
using System;
using System.Collections.Generic;

// simple and better RailProfile class
public class RailProfile
{
    public enum Axis { None, X, Y, Z }

    private readonly Axis _lockedAxis = Axis.None;
    private readonly float _lockedValue;
    private readonly float _allowedError;

    // _vArr[2k] and _vArr[2k+1] define a edge in cross section.
    // _vtArr[i] and _vnArr[i] describe a vertex _vArr[i].
    // however, _vArr[2k+1] and _vArr[2k+2] have no relation.
    private Vector3[] _vArr;
    private Vector2[] _vtArr;
    private Vector3[] _vnArr;

    public ReadOnlySpan<Vector3> Vertices => _vArr;
    public ReadOnlySpan<Vector2> UVs => _vtArr;
    public ReadOnlySpan<Vector3> Normals => _vnArr;

    public RailProfile(Axis lockedAxis, float lockedValue = 0, float allowedError = 0)
    {
        if(allowedError < 0) { allowedError = -allowedError; }

        _lockedAxis = lockedAxis;
        _lockedValue = lockedValue;
        _allowedError = allowedError;
    }

    private bool CheckConstraint(Vector3 v)
    {
        switch(_lockedAxis)
        {
            default: return false;
            case Axis.X: return MathF.Abs(v.X - _lockedValue) <= _allowedError;
            case Axis.Y: return MathF.Abs(v.Y - _lockedValue) <= _allowedError;
            case Axis.Z: return MathF.Abs(v.Z - _lockedValue) <= _allowedError;
        }
    }

    public void Load(string path)
    {
        ArrayMesh am = ResourceLoader.Load<ArrayMesh>(path);
        MeshDataTool mdt = new MeshDataTool();

        mdt.CreateFromSurface(am, 0);

        List<Vector3> vList = new List<Vector3>(2 * mdt.GetEdgeCount());
        List<Vector2> vtList = new List<Vector2>(2 * mdt.GetEdgeCount());
        List<Vector3> vnList = new List<Vector3>(2 * mdt.GetEdgeCount());
        
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
                        // fv0
                        vList.Add(faceVertexArr[fv0]);
                        vtList.Add(mdt.GetVertexUV(fvArr[fv0]));
                        vnList.Add(mdt.GetVertexNormal(fvArr[fv0]));

                        // fv1
                        vList.Add(faceVertexArr[fv1]);
                        vtList.Add(mdt.GetVertexUV(fvArr[fv1]));
                        vnList.Add(mdt.GetVertexNormal(fvArr[fv1]));
                    }
                }
            }
        }

        // convert list to array
        _vArr = vList.ToArray();
        _vtArr = vtList.ToArray();
        _vnArr = vnList.ToArray();
    }
}
