using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector to control spline points interactively in the scene view.
/// </summary>
[CustomEditor(typeof(Spline))]
public class SplineEditor : UnityEditor.Editor
{
    const float handleSize = 0.04f;
    const float pickSize = 0.06f;

    static readonly Dictionary<BezierControlPointMode, Color> modeColors = new Dictionary<BezierControlPointMode, Color>
    {
        { BezierControlPointMode.Aligned, Color.yellow },
        { BezierControlPointMode.Free, Color.white },
        { BezierControlPointMode.Mirrored, Color.cyan },
    };

    Spline spline;
    Transform handleTransform;
    Quaternion handleRotation;
    int selectedIndex = -1;

    public override void OnInspectorGUI()
    {
        spline = target as Spline;

        if (spline == null)
        {
            return;
        }

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            var loop = EditorGUILayout.Toggle("Loop", spline.Loop);

            if (check.changed)
            {
                Undo.RecordObject(spline, "Toggle Loop");
                EditorUtility.SetDirty(spline);
                spline.Loop = loop;
            }
        }

        var isPointSelected = selectedIndex >= 0 && selectedIndex < spline.ControlPointCount;

        if (isPointSelected)
        {
            DrawSelectedPointInspector();
        }

        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            EditorUtility.SetDirty(spline);
            spline.AddCurve();
        }
    }

    void OnSceneGUI()
    {
        spline = target as Spline;

        if (spline == null)
        {
            return;
        }

        handleTransform = spline.transform;

        handleRotation = Tools.pivotRotation == PivotRotation.Local
            ? handleTransform.rotation
            : Quaternion.identity;

        var point0 = ShowPoint(0);

        for (var i = 1; i < spline.ControlPointCount; i += 3)
        {
            var point1 = ShowPoint(i);
            var point2 = ShowPoint(i + 1);
            var point3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(point0, point1);
            Handles.DrawLine(point2, point3);

            Handles.DrawBezier(point0, point3, point1, point2, Color.white, null, 2f);
            point0 = point3;
        }
    }

    void DrawSelectedPointInspector()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            var point = EditorGUILayout.Vector3Field("Selected Point", spline.GetControlPoint(selectedIndex));

            if (check.changed)
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(selectedIndex, point);
            }
        }

        using (var check = new EditorGUI.ChangeCheckScope())
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel(" ");
            var mode = (BezierControlPointMode)EditorGUILayout.EnumPopup(spline.GetControlPointMode(selectedIndex));

            if (check.changed)
            {
                Undo.RecordObject(spline, "Change Point Mode");
                EditorUtility.SetDirty(spline);
                spline.SetControlPointMode(selectedIndex, mode);
            }
        }
    }

    Vector3 ShowPoint(int index)
    {
        var point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        var size = HandleUtility.GetHandleSize(point);

        if (index == 0)
        {
            size *= 2;
        }

        var controlPointMode = spline.GetControlPointMode(index);
        Handles.color = modeColors[controlPointMode];

        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {
            using var check = new EditorGUI.ChangeCheckScope();
            point = Handles.DoPositionHandle(point, handleRotation);

            if (check.changed)
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }

        return point;
    }
}