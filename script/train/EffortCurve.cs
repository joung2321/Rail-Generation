/// <summary>
/// tractive effort curve using F0(v) = k0, F1(v) = k1 / v
/// </summary>
public readonly record struct TractiveEffortCurve
{
    private readonly float _k0_N;
    private readonly float _k1_Nkph;
    private readonly float _v01_kph;

    /// <summary>
    /// defines tractive effort curve using F0(v) = k0, F1(v) = k1 / v
    /// </summary>
    /// <param name="k0_N">[N]</param>
    /// <param name="k1">[N * kph]</param>
    public TractiveEffortCurve(float k0_N, float k1_Nkph)
    {
        _k0_N = k0_N;
        _k1_Nkph = k1_Nkph;

        _v01_kph = k1_Nkph / k0_N;
    }

    public float F(float v_kph)
    {
        if(v_kph < 0) { v_kph = -v_kph; }

        if(v_kph <= _v01_kph) { return _k0_N; }
        else { return _k1_Nkph / v_kph; }
    }
}

/// <summary>
/// braking effort curve using F0(v) = k1 * (v / v01), F1(v) = k1, F2(v) = k2 / v
/// </summary>
public readonly record struct BrakingEffortCurve
{
    private readonly float _k1_N;
    private readonly float _k2_Nkph;
    private readonly float _v01_kph;
    private readonly float _v12_kph;

    /// <summary>
    /// defines braking effort curve using F0(v) = k1 * (v / v01), F1(v) = k1, F2(v) = k2 / v
    /// </summary>
    public BrakingEffortCurve(float v01_kph, float k1_N, float k2_Nkph)
    {
        _k1_N = k1_N;
        _k2_Nkph = k2_Nkph;

        _v01_kph = v01_kph;
        _v12_kph = k2_Nkph / k1_N;
    }

    public float F(float v_kph)
    {
        if(v_kph < 0) { v_kph = -v_kph; }

        if(v_kph == 0) { return _k1_N; }
        else if(v_kph < _v01_kph) { return _k1_N * v_kph / _v01_kph; }
        else if(v_kph < _v12_kph) { return _k1_N; }
        else { return _k2_Nkph / v_kph; }
    }
}