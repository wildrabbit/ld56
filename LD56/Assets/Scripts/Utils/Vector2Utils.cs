using UnityEngine;

public static class Vector2Utils
{
    public static Vector2 FromLengthAngle(float length, float angle)
    {
        float rads = Mathf.Deg2Rad * angle;
        return new Vector2(length * Mathf.Cos(rads)
            , length * Mathf.Sin(rads));
    }
}