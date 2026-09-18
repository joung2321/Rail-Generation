using System;
using Godot;

// class providing curves C(s) parameterized by arc length s
// C(0) is always zero vector.
// C'(0).X = 0, C'(0).Z < 0
public static class UnitSpeedCurve
{
    public readonly record struct Pose
    (
        Vector3 Position,
        Basis Axes,
        float Gradient_permille,
        float Roll
    );

    /* helper methods */
    private static Vector3 GetHorizontalRight(Vector3 front)
    {
        Vector3 right = front.Cross(Vector3.Up);
        return right;
    }

    private static float CalculateGradient(Vector3 front)
    {
        float horizontalLengthSquared = front.X * front.X + front.Z * front.Z;
        return front.Y / MathF.Sqrt(horizontalLengthSquared) * 1000;
    }

    private static Basis CalculateAxes(Vector3 front, Vector3 right)
    {
        return new Basis(right, right.Cross(front), -front).Orthonormalized();
    }

    /* curves C(s) parameterized by arc length s */
    // let P = endPoint, final_s = |P|
    // then, C(s) = s * (P / |P|)
    public static Pose Line(float s, float final_s_xz, float final_h)
    {
        Vector3 endPoint = new Vector3(0, final_h, -final_s_xz);
        Basis axes = CalculateAxes(endPoint, Vector3.Right);

        // endPoint.Normalized() == -axes.Column2
        return new Pose(s * -axes.Column2, axes, CalculateGradient(endPoint), 0);
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

        return new Pose(position, axes, CalculateGradient(front), 0);
    }

    // helix C(theta) = <-r * cos(theta), s * sin(pitch), -r * sin(theta)> + <r, 0, 0>
    // s = r * theta / cos(pitch) => C(theta).Y = r * theta * tan(pitch)
    // C'(theta)      = <r * sin(theta), r * tan(pitch), -r * cos(theta)>
    // C'(theta) / r  = <sin(theta), tan(pitch), -cos(theta)>
    public static Pose HorizontalCurve(float s, float r, float roll, float pitch)
    {
        float theta = s * MathF.Cos(pitch) / r;
        float sin_th = MathF.Sin(theta);
        float cos_th = MathF.Cos(theta);

        Vector3 position = new Vector3(r - r * cos_th, s * MathF.Sin(pitch), -r * sin_th);
        Vector3 front = new Vector3(sin_th, MathF.Tan(pitch), -cos_th).Normalized();

        if(r < 0) { roll = -roll; }
        Vector3 right = GetHorizontalRight(front).Rotated(front, roll);

        Basis axes = CalculateAxes(front, right);

        return new Pose(position, axes, CalculateGradient(front), roll);
    }
    
    // A^2 = R * L
    public static Pose Clothoid(float s, float A, float final_r, float final_roll, float pitch)
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
        float roll = final_roll * (final_r / r);

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
        right = front.Cross(Vector3.Up).Rotated(front, roll); // XYZ space
        Basis axes = CalculateAxes(front, right);
        
        return new Pose(position, axes, 0, roll);
    }
}
