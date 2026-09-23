using Godot;
using System;

public static class TrackConnector
{
    public enum Terminal { Initial, Final }

    // extracts yawing from a basis
    private static Basis Yawing(Basis basis)
    {
        Vector3 back_xz = basis.Column2 with { Y = 0 };

        Basis yawing = new Basis
        (
            Vector3.Up.Cross(back_xz),
            Vector3.Up,
            back_xz
        );

        return yawing.Orthonormalized();
    }

    private static Basis Reverse(Basis basis)
    {
        return new Basis
        (
            -basis.Column0,
            basis.Column1,
            -basis.Column2
        );
    }

    /// <param name="dst">a fixed TrackPiece</param>
    /// <param name="src">a TrackPiece which will be snapped to dst</param>
    public static void Connect(TrackPiece dst, Terminal dst_terminal, TrackPiece src, Terminal src_terminal)
    {
        if(dst.GetParent() != src.GetParent())
        {
            GD.Print("dst and src MUST have a same parent!!");
            return;
        }

        switch((dst_terminal, src_terminal))
        {
            case (Terminal.Final, Terminal.Initial):
            src.Basis = dst.Basis * Yawing(dst.FinalPose.Axes) * Yawing(src.InitialPose.Axes).Transposed();
            src.Position = dst.Transform * dst.FinalPose.Position - src.Basis * src.InitialPose.Position;
            break;

            case (Terminal.Final, Terminal.Final):
            src.Basis = dst.Basis * Yawing(dst.FinalPose.Axes) * Yawing(Reverse(src.FinalPose.Axes)).Transposed();
            src.Position = dst.Transform * dst.FinalPose.Position - src.Basis * src.FinalPose.Position;
            break;

            case (Terminal.Initial, Terminal.Initial):
            src.Basis = dst.Basis * Yawing(dst.InitialPose.Axes) * Yawing(Reverse(src.InitialPose.Axes)).Transposed();
            src.Position = dst.Transform * dst.InitialPose.Position - src.Basis * src.InitialPose.Position;
            break;

            case (Terminal.Initial, Terminal.Final):
            src.Basis = dst.Basis * Yawing(dst.InitialPose.Axes) * Yawing(src.FinalPose.Axes).Transposed();
            src.Position = dst.Transform * dst.InitialPose.Position - src.Basis * src.FinalPose.Position;
            break;
        }
    }
}
