using Godot;
using System;

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

    public Terminal PrevTerminal { get; private set; } = Terminal.None;
    public Terminal NextTerminal { get; private set; } = Terminal.None;

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

        Pose[] poses = new Pose[poseCount];

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
        if(terminal.HasFlag(Terminal.Initial) && Prev != null)
        {
            if(Prev.Prev == this) { Prev.Prev = null; Prev.PrevTerminal = Terminal.None; }
            if(Prev.Next == this) { Prev.Next = null; Prev.NextTerminal = Terminal.None; }
        }

        if(terminal.HasFlag(Terminal.Final) && Next != null)
        {
            if(Next.Prev == this) { Next.Prev = null; Next.PrevTerminal = Terminal.None; }
            if(Next.Next == this) { Next.Next = null; Next.NextTerminal = Terminal.None; }
        }
    }

    public override void _ExitTree()
    {
        Unlink(Terminal.Initial | Terminal.Final);
    }

    // snaps a terminal to the other track piece
    public void Snap(Terminal this_terminal, TrackPiece anchor, Terminal anchor_terminal)
    {
        if(anchor == null || GetParent() != anchor.GetParent()) { return; }

        Pose pose_this, pose_anchor; // pose of this_terminal and anchor_terminal
        Basis axes_this;

        pose_this = this_terminal == Terminal.Initial? InitialPose: FinalPose;
        pose_anchor = anchor_terminal == Terminal.Initial? anchor.InitialPose: anchor.FinalPose;
        
        axes_this = pose_this.Axes;
        if(this_terminal == anchor_terminal) { axes_this = Reverse(axes_this); }

        Basis = anchor.Basis * Yawing(pose_anchor.Axes) * Yawing(axes_this).Transposed();
        Position = anchor.Transform * pose_anchor.Position - Basis * pose_this.Position;
    }
}
