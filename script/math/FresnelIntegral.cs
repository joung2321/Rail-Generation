using System;

public static class FresnelIntegral
{
    // non-zero coefficients of Maclaurin series
    private static float[] _s_coefs; // Fresnel integral S(x)
    private static float[] _c_coefs; // Fresnel integral C(x)

    // calculates S(x)
    public static float S(float x)
    {
        if(_s_coefs != null)
        {
            float x2 = x * x;
            float x3 = x * x2;
            float x4 = x2 * x2;

            float ans = 0;
            for(int i=_s_coefs.Length-1; i>=0; i--)
            {
                ans *= x4;
                ans += _s_coefs[i];
            }

            ans *= x3;
            return ans;
        }
        else { throw new InvalidOperationException("Call FresnelIntegral.Approximate() before using FresnelIntegral.S()"); }
    }

    // calculates C(x)
    public static float C(float x)
    {
        if(_c_coefs != null && _c_coefs.Length > 0)
        {
            float x2 = x * x;
            float x4 = x2 * x2;

            float ans = 0;
            for(int i=_c_coefs.Length-1; i>=0; i--)
            {
                ans *= x4;
                ans += _c_coefs[i];
            }
            
            ans *= x;
            return ans;
        }
        else { throw new InvalidOperationException("Call FresnelIntegral.Approximate() before using FresnelIntegral.C()"); }
    }

    public static void Approximate(int size)
    {
        if(size > 0)
        {
            // create array
            _s_coefs = new float[size];
            _c_coefs = new float[size];

            // first element
            float s_coef = 1f / 3;
            float c_coef = 1f;

            // fill array
            _s_coefs[0] = s_coef;
            _c_coefs[0] = c_coef;
            
            for(int n=1; n<size; n++)
            {
                // S(x)
                s_coef *= -(4*n - 1) / (float)((4*n + 3) * 2*n * (2*n + 1));
                _s_coefs[n] = s_coef;

                // C(x)
                c_coef *= -(4*n - 3) / (float)((4*n + 1) * (2*n - 1) * 2*n);
                _c_coefs[n] = c_coef;
            }
        }
        else { _s_coefs = _c_coefs = null; }
    }
}
