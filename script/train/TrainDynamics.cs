using System;
using Godot;

public class TrainDynamics
{
    private const float g = 9.80665f; // acceleration of gravity [m/s^2]

    private float _G_m; // gauge [m]

    // starting resistance
    private float _SR_Npt;
    private float _SR3kph_Npt; // pre-calculated starting resistance at v = 3 km/h

    // propulsion resistance
    private float _a, _b, _c;

    // curving resistance
    private float _mu; // coefficient of friction

    public TrainDynamics(float G_m, float SR_Npt, float a, float b, float c, float mu)
    {
        _G_m = G_m;
        _SR_Npt = SR_Npt;
        _a = a;
        _b = b;
        _c = c;
        _mu = mu;

        _SR3kph_Npt = a + b * 3 + c * 9;
    }

    /// <summary>
    /// calculates propulsion resistance [N/t]
    /// </summary>
    public float CalculatePR(float v_kph)
    {
        if(v_kph < 0) { v_kph = -v_kph; }

        if(v_kph <= 3f) // starting resistance
        {
            float t = v_kph / 3f;
            return _SR_Npt * (1 - t) + _SR3kph_Npt * t;
        }
        else // propulsion resistance
        {
            return _a + _b * v_kph + _c * v_kph * v_kph;
        }
    }

    /// <summary>
    /// calculates grade resistance [N/t]
    /// </summary>
    public float CalculateGR(float i_permille)
    {
        return g * i_permille;
    }

    /// <summary>
    /// calculates curving resistance [N/t]
    /// </summary>
    /// <param name="L_m">wheelbase</param>
    public float CalculateCR(float r_m, float L_m)
    {
        if(r_m <= 0) { return 0; }
        return g * 1000 * _mu * (_G_m + L_m) / (2 * r_m);
    }

    /// <summary>
    /// calculates velocity after delta [s]
    /// </summary>
    /// <param name="traction_N">(-) : brake, 0 : neutral, (+) : power</param>
    public float UpdateVelocity(double delta_s, Pose pose, float v_kph, Reverser reverser, float traction_N, float L_m, float W_t)
    {
        float dt_s = (float)delta_s;

        float gr_N = W_t * CalculateGR(pose.Gradient_permille);
        float absFriction_N = W_t * (CalculatePR(v_kph) + CalculateCR(pose.R, L_m)); // always greater than 0

        // i) brake or power
        if(traction_N < 0) // brake: consider traction as friction
        {
            absFriction_N -= traction_N;
            traction_N = 0;
        }

        // ii) consider reverser
        if(reverser == Reverser.Neutral) { traction_N = 0; }
        else if(reverser == Reverser.Reverse) { traction_N = -traction_N; }

        // iii) consider gradient as traction
        traction_N -= gr_N;

        /* update velocity */
        if(v_kph != 0)
        {
            float prev_v = v_kph;

            // m/s  = N / kg * s
            // km/h = 1000m / 3600s = m/s / 3.6
            // 3.6 km/h = 1 m/s
            v_kph += 3.6f * (traction_N - Math.Sign(v_kph) * absFriction_N) / (1000 * W_t) * dt_s;

            if(Math.Sign(v_kph) == Math.Sign(prev_v)) { return v_kph; }
            else { return 0; }
        }
        else // v == 0
        {
            float absSumF = Math.Abs(traction_N) - absFriction_N;

            if(absSumF > 0) { return Math.Sign(traction_N) * 3.6f * absSumF / (1000 * W_t) * dt_s; } // 3.6 km/h = 1 m/s
            else { return 0; }
        }
    }
}
