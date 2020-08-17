using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SquarePacking : EditorWindow
{
    public List<float> ratios = new List<float>() { .5f, .3f, .2f, .1f };
    List<Rect> rects = new List<Rect>();
    Rect drawingArea;


    void OnGUI()
    {
        // Get rects from test ratios.
        List<Rect> ratiosRects = new List<Rect>();
        foreach (float ratio in ratios)
            ratiosRects.Add(RatioToRect(ratio));

        // Pack rects.
        if (rects.Count < 1)
        {
            RectPack packingResult = RectPacker.PackOrderedRects(ratiosRects);
            float areaRatio = 1 / packingResult.area.width;
            foreach (Rect packedRect in packingResult.rects)
                rects.Add(new Rect(packedRect.position * areaRatio, packedRect.size * areaRatio));
        }

        // Draw title and area.
        GUILayout.Label("UVs", EditorStyles.boldLabel);
        Rect uVDisplayRect = EditorGUILayout.GetControlRect(GUILayout.Width(300), GUILayout.Height(300));
        EditorGUI.DrawRect(new Rect(uVDisplayRect.position, uVDisplayRect.size), Color.grey);

        // Draw rects.
        Vector2 startPosition = new Vector2(uVDisplayRect.x, uVDisplayRect.y + uVDisplayRect.height);
        float displayArea = uVDisplayRect.width * uVDisplayRect.height;
        for (int i = 0; i < rects.Count; i++)
            GUI.Label(new Rect(rects[i].position *uVDisplayRect.width + uVDisplayRect.position, rects[i].size * uVDisplayRect.width), i.ToString(), GUI.skin.textArea);

        if (GUILayout.Button("Reset"))
            rects.Clear();

        // Ratios property.
        SerializedObject so = new SerializedObject(this);
        SerializedProperty ratiosProperty = so.FindProperty("ratios");
        EditorGUILayout.PropertyField(ratiosProperty);
        so.ApplyModifiedProperties();
    }


    [MenuItem("Window/Tools/Square Packing")]
    static void Open()
    {
        GetWindow(typeof(SquarePacking)).titleContent = new GUIContent(typeof(SquarePacking).Name);
    }

    Rect RatioToRect(float ratio)
    {
        return new Rect(0, 0, ratio, ratio);
    }
}
