namespace RailGeneration;

using Godot;
using System;
using RailGeneration.Common;

using static BasisMath;

/// <typeparam name="TCurveShape">a set of parameters describing a curve</typeparam>
public abstract partial class TrackPiece : MeshInstance3D
{
    // pre-calculated values
    public float TotalLength { get; private set; }
    public Pose InitialPose { get; private set; } // C(0)
    public Pose FinalPose { get; private set; } // C(TotalLength)

    // doubly linked list
    // TrackPiece <- [ Prev | InitialPose - TrackPiece - FinalPose | Next ] -> TrackPiece
    public TrackPiece Prev { get; private set; } = null;
    public TrackPiece Next { get; private set; } = null;

    public Terminal PrevTerminal { get; private set; }
    public Terminal NextTerminal { get; private set; }

    /// <summary>
    /// TODO: calculate and return TotalLength using this.CurveShape
    /// </summary>
    /// <param name="curveShape">a set of parameters describing this curve</param>
    protected abstract float CalculateTotalLength();

    /// <summary>
    /// a unit speed curve
    /// </summary>
    /// <param name="s">arc length</param>
    public abstract Pose C(float s);

    /// <summary>
    /// hook method for Bake()
    /// </summary>
    protected virtual void _Bake() {}

    /// <summary>
    /// pre-calculates TotalLength, InitialPose, FinalPose
    /// </summary>
    /// <returns>returns itself</returns>
    public TrackPiece Bake()
    {
        _Bake();

        TotalLength = CalculateTotalLength();
        InitialPose = C(0);
        FinalPose = C(TotalLength);

        return this;
    }

    public virtual void GenerateMesh(RailProfile profile, float ds = 0.5f)
    {
        int poseCount = (int)Math.Ceiling(TotalLength / ds) + 1;
        if(poseCount < 2) { return; }

        Pose[] poses = new Pose[poseCount];
        float[] cumulativeDistances = new float[poseCount];

        // fill array
        poses[0] = InitialPose;
        cumulativeDistances[0] = 0;

        for(int i=1; i<poseCount; i++)
        {
            float s = ds * i;

            if(s < TotalLength)
            {
                poses[i] = C(s);
                cumulativeDistances[i] = s;
            }
            else
            {
                poses[i] = FinalPose;
                cumulativeDistances[i] = TotalLength;
            }
        }

        Mesh = RailMeshGenerator.GenerateMesh(profile, poses, cumulativeDistances);
    }

    /* doubly linked list */
    public void Link(Terminal this_terminal, TrackPiece other, Terminal other_terminal)
    {
        if(other == null) { return; }
        
        TrackPiece ptr_tt, ptr_ot; // pointer of this_terminal and other_terminal

        ptr_tt = this_terminal  == Terminal.Initial? Prev: Next;
        ptr_ot = other_terminal == Terminal.Initial? other.Prev: other.Next;

        if(ptr_tt != null || ptr_ot != null) { return; }

        // link 2 track pieces
        // this
        if(this_terminal == Terminal.Initial)
        {
            Prev = other;
            PrevTerminal = other_terminal;
        }
        else
        {
            Next = other;
            NextTerminal = other_terminal;
        }

        // other
        if(other_terminal == Terminal.Initial)
        {
            other.Prev = this;
            other.PrevTerminal = this_terminal;
        }
        else
        {
            other.Next = this;
            other.NextTerminal = this_terminal;
        }
    }

    public void Unlink(Terminal terminal)
    {
        if(terminal == Terminal.Initial && Prev != null)
        {
            if(Prev.Prev == this) { Prev.Prev = null; }
            if(Prev.Next == this) { Prev.Next = null; }
        }

        if(terminal == Terminal.Final && Next != null)
        {
            if(Next.Prev == this) { Next.Prev = null; }
            if(Next.Next == this) { Next.Next = null; }
        }
    }

    public override void _ExitTree()
    {
        Unlink(Terminal.Initial);
        Unlink(Terminal.Final);
    }

    // snaps a terminal to the other track piece
    public void Snap(Terminal this_terminal, TrackPiece anchor, Terminal anchor_terminal)
    {
        if(anchor == null || anchor == this) { return; }

        Pose pose_this, pose_anchor; // pose of this_terminal and anchor_terminal
        Basis axes_this;

        pose_this = this_terminal == Terminal.Initial? InitialPose: FinalPose;
        pose_anchor = anchor_terminal == Terminal.Initial? anchor.InitialPose: anchor.FinalPose;
        
        axes_this = pose_this.Axes;
        if(this_terminal == anchor_terminal) { axes_this = Reverse(axes_this); }

        // GlobalBasis = anchor.GlobalBasis * Yawing(pose_anchor.Axes) * Yawing(axes_this).Transposed();
        GlobalBasis = anchor.GlobalBasis * pose_anchor.Axes * axes_this.Transposed();
        GlobalPosition = anchor.GlobalTransform * pose_anchor.Position - Basis * pose_this.Position;
    }
}
