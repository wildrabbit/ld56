using UnityEngine;

public static class Vector2Utils
{
    public static Vector2 FromLengthAngle(float length, float angleDegrees)
    {
        float angleRads = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(length * Mathf.Cos(angleRads)
            , length * Mathf.Sin(angleRads));
    }
}