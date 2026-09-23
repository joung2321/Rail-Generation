using System;
using Godot;

// class providing curves C(s) parameterized by arc length s
// C(0) is always zero vector.
// C'(0).X = 0, C'(0).Z < 0
public static class UnitSpeedCurve
{
    public readonly record struct Pose
    {
        public readonly Vector3 Position;
        public readonly Basis Axes;
        public readonly float Bank;

        public Pose(Vector3 position, Basis axes, float bank)
        {
            Position = position;
            Axes = axes;
            Bank = bank; // bank > 0: positive camber, bank < 0: negative camber
        }

        public float Gradient_permille => Axes.Column0.Y / MathF.Sqrt(Axes.Column0.X * Axes.Column0.X + Axes.Column0.Z * Axes.Column0.Z) * 1000;
    };

    /* helper methods */
    private static Vector3 GetHorizontalRight(Vector3 front)
    {
        return front.Cross(Vector3.Up);
    }

    private static Basis CalculateAxes(Vector3 front, Vector3 right)
    {
        return new Basis(right, right.Cross(front), -front).Orthonormalized();
    }

    /* length of curves C(s) */
    public static float LengthOfLine(float final_s_xz, float final_h)
    {
        return MathF.Sqrt(final_s_xz * final_s_xz + final_h * final_h);
    }

    public static float LengthOfVerticalCurve(float r, float final_theta)
    {
        if(r < 0) { r = -r; }
        return r * final_theta;
    }

    public static float LengthOfHorizontalCurve(float r, float final_theta, float pitch)
    {
        if(r < 0) { r = -r; }
        return r * final_theta / MathF.Cos(pitch);
    }

    public static float LengthOfClothoid(float A, float final_r, float pitch)
    {
        if(final_r < 0) { final_r = -final_r; }
        return A * A / final_r / MathF.Cos(pitch); // A^2 = R * L
    }

    /* curves C(s) parameterized by arc length s */
    // let P = endPoint, final_s = |P|
    // then, C(s) = s * (P / |P|)
    public static Pose Line(float s, float final_s_xz, float final_h)
    {
        Vector3 endPoint = new Vector3(0, final_h, -final_s_xz);
        Basis axes = CalculateAxes(endPoint, Vector3.Right);

        // endPoint.Normalized() == -axes.Column2
        return new Pose(s * -axes.Column2, axes, 0);
    }

    // arc of a circle in YZ plane whose center is (0, r, 0) and radius is r
    // parameterize curve C using theta = s / r:
    // C(theta)      = <0, r - r * cos(theta), -r * sin(theta)>
    // C'(theta)     = r * <0, sin(theta), -cos(theta)>
    // C'(theta) / r = <0, sin(theta), -cos(theta)> is a unit tangent vector
    public static Pose VerticalCurve(float s, float r)
    {
        float theta = s / r;
        float sin_th = MathF.Sin(theta);
        float cos_th = MathF.Cos(theta);

        Vector3 position = new Vector3(0, r - r * cos_th, -r * sin_th);
        Vector3 front = new Vector3(0, sin_th, -cos_th);
        Basis axes = CalculateAxes(front, Vector3.Right);

        return new Pose(position, axes, 0);
    }

    // helix C(theta) = <-r * cos(theta), s * sin(pitch), -r * sin(theta)> + <r, 0, 0>
    // s = r * theta / cos(pitch) => C(theta).Y = r * theta * tan(pitch)
    // C'(theta)      = <r * sin(theta), r * tan(pitch), -r * cos(theta)>
    // C'(theta) / r  = <sin(theta), tan(pitch), -cos(theta)>
    public static Pose HorizontalCurve(float s, float r, float bank, float pitch)
    {
        float theta = s * MathF.Cos(pitch) / r;
        float sin_th = MathF.Sin(theta);
        float cos_th = MathF.Cos(theta);

        Vector3 position = new Vector3(r - r * cos_th, s * MathF.Sin(pitch), -r * sin_th);
        Vector3 front = new Vector3(sin_th, MathF.Tan(pitch), -cos_th).Normalized();
        Vector3 right = GetHorizontalRight(front).Rotated(front, r > 0? bank: -bank);
        Basis axes = CalculateAxes(front, right);

        return new Pose(position, axes, bank);
    }
    
    // A^2 = R * L
    public static Pose Clothoid(float s, float A, float final_r, float final_bank, float pitch)
    {
        float s_xz = s * MathF.Cos(pitch); // horizontal arc length
        float A2 = A * A;
        
        // 1/a
        // = sqrt(2 * R * L)
        // = sqrt(2) * sqrt(R * L)
        // = sqrt(2) * sqrt(A^2)
        // = sqrt(2) * A
        float inv_a = MathF.Sqrt(2) * A;

        // 1/r = C * s
        // A = sqrt(r * s) => C = 1 / A^2
        // r = A^2 / s
        float r = (s_xz > 0)? A2 / s_xz: float.PositiveInfinity;

        // apply smoothstep to bank
        float t = s / (A2 / final_r); // t = s / L
        if(t < 0) { t = -t; }
        float bank = final_bank * t * t * (3 - 2 * t);

        // d/ds theta = 1/r = C * s => theta = 1/2 * C * s^2
        // theta = s^2 / (2 * A^2)
        float theta = s_xz * s_xz / (2 * A2);

        Vector3 position = new Vector3(inv_a * FresnelIntegral.S(s_xz/inv_a), s * MathF.Sin(pitch), -inv_a * FresnelIntegral.C(s_xz/inv_a));
        Vector3 front = new Vector3(MathF.Sin(theta), 0, -MathF.Cos(theta)); // XZ plane
        Vector3 right = front.Cross(Vector3.Up).Normalized(); // XZ plane

        if(final_r < 0)
        {
            position.X = -position.X;
            front.X = -front.X;
        }
        
        front = front.Rotated(right, pitch).Normalized(); // XYZ space
        right = front.Cross(Vector3.Up).Rotated(front, final_r >= 0? bank: -bank); // XYZ space
        Basis axes = CalculateAxes(front, right);
        
        return new Pose(position, axes, bank);
    }
}
