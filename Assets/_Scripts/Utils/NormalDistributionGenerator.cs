using UnityEngine;

public static class NormalDistributionGenerator
{
    private const float MEAN = 6.5f;
    private const float STD_DEV = 0.5f;
    private static bool hasSpare = false;
    private static float spare;

    public static float NextValue(float min, float max)
    {
        if (hasSpare)
        {
            hasSpare = false;
            return MEAN + spare * STD_DEV;
        }

        float u1 = Random.value;
        float u2 = Random.value;
        float r = Mathf.Sqrt(-2.0f * Mathf.Log(u1));
        float theta = 2.0f * Mathf.PI * u2;

        spare = r * Mathf.Sin(theta);
        hasSpare = true;

        float sampledValue = MEAN + r * Mathf.Cos(theta) * STD_DEV;
        // return sampledValue;
        return Mathf.Max(min, Mathf.Min(sampledValue, max));
    }
}