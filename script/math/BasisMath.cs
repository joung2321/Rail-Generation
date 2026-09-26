using Godot;

public static class BasisMath
{
    // extracts yawing from a basis
    public static Basis Yawing(Basis basis)
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

    public static Basis Reverse(Basis basis)
    {
        return new Basis
        (
            -basis.Column0,
            basis.Column1,
            -basis.Column2
        );
    }
}
