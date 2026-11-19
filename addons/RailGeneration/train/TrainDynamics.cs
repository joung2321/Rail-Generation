using System;
using Godot;

public class TrainDynamics
{
    private const float g = 9.80665f; // acceleration of gravity [m/s^2]

    // shape
    private float _G_m; // gauge [m]
    private float _L_m; // wheelbase in a bogie [m]
    private float _W_t; // weight [t]

    // starting resistance
    private float _SR_Npt; // starting resistance at v = 0 km/h

    // propulsion resistance
    private float _a, _b, _c;
    private float _PR3kph_Npt; // pre-calculated propulsion resistance at v = 3 km/h

    // curving resistance
    private float _mu; // coefficient of friction [1; dimensionless]

    // pre-calculates propulsion resistance at v = 3 km/h
    private void BakePR3kph()
    {
        _PR3kph_Npt = g * (_a + _b * 3 + _c * 9 / _W_t);
    }

    public TrainDynamics(float G_m, float L_m, float W_t, float SR_Npt, float a, float b, float c, float mu)
    {
        // shape
        _G_m = G_m;
        _L_m = L_m;
        _W_t = W_t;

        // SR
        _SR_Npt = SR_Npt;

        // PR
        _a = a;
        _b = b;
        _c = c;

        // CR
        _mu = mu;

        // pre-calculate propulsion resistance at v = 3 km/h
        BakePR3kph();
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
            return _SR_Npt * (1 - t) + _PR3kph_Npt * t;
        }
        else // propulsion resistance
        {
            return g * (_a + _b * v_kph + _c * v_kph * v_kph / _W_t);
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
    public float CalculateCR(float r_m)
    {
        if(r_m <= 0) { return 0; }
        return g * 1000 * _mu * (_G_m + _L_m) / (2 * r_m);
    }
    
    /// <summary>
    /// calculates velocity [km/h] after delta [s]
    /// </summary>
    public float UpdateVelocity(double delta_s, float v_kph, float absFriction_N, float traction_N)
    {
        if(absFriction_N < 0) { absFriction_N = -absFriction_N; }

        float dt_s = (float)delta_s;

        if(v_kph != 0)
        {
            float prev_v = v_kph;

            // dv = F / m * dt
            // 3.6 km/h = 1 m/s
            v_kph += 3.6f * (traction_N - Math.Sign(v_kph) * absFriction_N) / (1000 * _W_t) * dt_s;

            if(prev_v * v_kph > 0) { return v_kph; }
            else { return 0; }
        }
        else
        {
            float absSumF = Math.Abs(traction_N) - absFriction_N;

            if(absSumF > 0) { return Math.Sign(traction_N) * 3.6f * absSumF / (1000 * _W_t) * dt_s; }
            else { return 0; }
        }
    }

    /// <summary>
    /// calculates velocity [km/h] after delta [s]
    /// </summary>
    /// <param name="commandForce_N">(-) : brake, 0 : neutral, (+) : power</param>
    public float UpdateVelocity(double delta_s, Pose pose, float v_kph, Reverser reverser, float commandForce_N)
    {
        float absFriction_N = _W_t * (CalculatePR(v_kph) + CalculateCR(pose.R));
        float gr_N = _W_t * CalculateGR(pose.Gradient_permille);

        // 1. command force as brake
        if(commandForce_N < 0)
        {
            absFriction_N -= commandForce_N;
            commandForce_N = 0;
        }

        // 2. consider reverser
        if(reverser == Reverser.Neutral) { commandForce_N = 0; }
        if(reverser == Reverser.Reverse) { commandForce_N = -commandForce_N; }
        
        // 3. consider gradient resistance as traction
        commandForce_N -= gr_N;

        return UpdateVelocity(delta_s, v_kph, absFriction_N, commandForce_N);
    }
}
