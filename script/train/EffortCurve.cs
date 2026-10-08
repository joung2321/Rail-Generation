/// <summary>
/// normalized tractive effort curve with max(F_tractive) = 1
/// </summary>
public readonly record struct NormalizedTractiveEffortCurve
{
    private readonly float _v1_kph;
    private readonly float _v2_kph;
    private readonly float _k2_kph;

    public NormalizedTractiveEffortCurve(float v1_kph, float v2_kph)
    {
        _v1_kph = v1_kph;
        _v2_kph = v2_kph;
        _k2_kph = v1_kph * v2_kph;
    }

    public float F(float v_kph)
    {
        if(v_kph < 0) { v_kph = -v_kph; }

        if(v_kph <= _v1_kph) { return 1f; }
        else if(v_kph <= _v2_kph) { return _v1_kph / v_kph; }
        else { return _k2_kph / (v_kph * v_kph); }
    }
}

/// <summary>
/// normalized braking effort curve with max(F_braking) = 1
/// </summary>
public readonly record struct NormalizedBrakingEffortCurve
{
    private readonly float _v1_kph;
    private readonly float _v2_kph;

    public NormalizedBrakingEffortCurve(float v1_kph, float v2_kph)
    {
        _v1_kph = v1_kph;
        _v2_kph = v2_kph;
    }

    public float F(float v_kph)
    {
        if(v_kph < 0) { v_kph = -v_kph; }

        if(v_kph == 0) { return 1; }
        else if(v_kph < _v1_kph) { return v_kph / _v1_kph; }
        else if(v_kph <= _v2_kph) { return 1f; }
        else { return _v2_kph / v_kph; }
    }
}