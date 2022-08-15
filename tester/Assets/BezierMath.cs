using UnityEngine;

/// <summary>
/// Math functions for calculating Bezier curves.
/// </summary>
public static class BezierMath
{
    public static Vector3 FirstDerivative(Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3, float t)
    {
        t = Mathf.Clamp01(t);
        var oneMinusT = 1f - t;

        return
            3f * oneMinusT * oneMinusT * (point1 - point0) +
            6f * oneMinusT * t * (point2 - point1) +
            3f * t * t * (point3 - point2);
    }

    public static Vector3 Point(Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3, float t)
    {
        t = Mathf.Clamp01(t);
        var oneMinusT = 1f - t;

        return
            oneMinusT * oneMinusT * oneMinusT * point0 +
            3f * oneMinusT * oneMinusT * t * point1 +
            3f * oneMinusT * t * t * point2 +
            t * t * t * point3;
    }
}