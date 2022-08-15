using System;
using UnityEngine;

/// <summary>
/// A path constructed from a series of Bezier curves.
/// Adapted from Catlike Coding's "Curves and Splines" tutorial.
/// https://catlikecoding.com/unity/tutorials/curves-and-splines/
/// </summary>
public class Spline : MonoBehaviour
{
    [SerializeField] Vector3[] points;
    [SerializeField] BezierControlPointMode[] modes;
    [SerializeField] bool loop;

    public int ControlPointCount => points.Length;
    public int CurveCount => (points.Length - 1) / 3;

    public bool Loop
    {
        get => loop;
        set
        {
            loop = value;

            if (value)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            var delta = point - points[index];

            if (loop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }

                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }

        points[index] = point;
        EnforceMode(index);
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        var modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;

        if (loop)
        {
            if (modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if (modeIndex == modes.Length - 1)
            {
                modes[0] = mode;
            }
        }

        EnforceMode(index);
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public Vector3 GetPoint(float t)
    {
        int i;

        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        var bezierPoint = BezierMath.Point(points[i], points[i + 1], points[i + 2], points[i + 3], t);
        return transform.TransformPoint(bezierPoint);
    }

    public void AddCurve()
    {
        Array.Resize(ref modes, modes.Length + 1);
        Array.Resize(ref points, points.Length + 3);

        var point = points[points.Length - 1];
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }

    public void Reset()
    {
        points = new[]
        {
            new Vector3(1, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(3, 0, 0),
            new Vector3(4, 0, 0),
        };

        modes = new[]
        {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free,
        };
    }

    void EnforceMode(int index)
    {
        var modeIndex = (index + 1) / 3;
        var mode = modes[modeIndex];

        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
        {
            return;
        }

        var middleIndex = modeIndex * 3;
        int fixedIndex;
        int enforcedIndex;

        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;

            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }

            enforcedIndex = middleIndex + 1;

            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;

            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }

            enforcedIndex = middleIndex - 1;

            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }

        var middle = points[middleIndex];
        var enforcedTangent = middle - points[fixedIndex];

        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }

        points[enforcedIndex] = middle + enforcedTangent;
    }

    Vector3 GetVelocity(float t)
    {
        int i;

        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        var bezierFirstDerivative = BezierMath.FirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t);
        return transform.TransformPoint(bezierFirstDerivative) - transform.position;
    }
}