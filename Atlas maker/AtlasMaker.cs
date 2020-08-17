using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AtlasMaker : EditorWindow
{
    class TargetMesh
    {
        public MeshFilter meshFilter;
        public float volume;
        public float ratio;

        public TargetMesh(MeshFilter meshFilter)
        {
            this.meshFilter = meshFilter;
            volume = VolumeOfMesh(meshFilter.sharedMesh);
        }
    }
    List<TargetMesh> meshes = new List<TargetMesh>();
    float totalVolume;
    Vector2 scroll;


    void OnGUI()
    {
        if (meshes.Count < 1)
            GetMeshFilters();

        // Stop if no meshes.
        if (meshes.Count < 1)
        {
            GUILayout.Label("Select target meshes.");
            return;
        }

        GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

        // Draw current meshes.
        GUILayout.Label("Meshes", EditorStyles.boldLabel);
        for (int i = 0; i < meshes.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(i.ToString(), GUILayout.Width(15));
            meshes[i].meshFilter = (MeshFilter)EditorGUILayout.ObjectField(meshes[i].meshFilter, typeof(MeshFilter), true);
            float percent = (float)Math.Round(meshes[i].ratio * 100, 1);
            GUILayout.Label((percent > 0 ? percent.ToString() : "< 0.1") + "%", GUILayout.Width(43));
            GUILayout.EndHorizontal();
        }

        // Draw UVs header and area.
        GUILayout.Label("UVs", EditorStyles.boldLabel);
        scroll = GUILayout.BeginScrollView(scroll);
        Rect uVDisplayRect = EditorGUILayout.GetControlRect(GUILayout.Width(300), GUILayout.Height(300));
        EditorGUI.DrawRect(new Rect(uVDisplayRect.position, uVDisplayRect.size), Color.grey);

        Vector2 startPosition = new Vector2(uVDisplayRect.x, uVDisplayRect.y + uVDisplayRect.height);
        Debug.Log(startPosition);
        float totalWidth = 0;
        float displayArea = uVDisplayRect.width * uVDisplayRect.height;
        for (int i = 0; i < meshes.Count; i++)
        {
            float targetArea = displayArea * meshes[i].ratio;
            float targetWidth = Mathf.Sqrt(targetArea);
            Vector2 rectOrigin = new Vector2(startPosition.x + totalWidth, startPosition.y - targetWidth);
            GUI.Label(new Rect(rectOrigin, Vector2.one * targetWidth), i.ToString(), GUI.skin.textArea);
            totalWidth += targetWidth;
        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        // "Create" button.
        if (GUILayout.Button("Create atlas"))
            Debug.Log("hey");
    }

    void OnSelectionChange()
    {
        Repaint();
        GetMeshFilters();
    }


    void GetMeshFilters()
    {
        // Reset meshes and total volume.
        meshes.Clear();
        totalVolume = 0;

        // Get meshes and total volume.
        for (int i = 0; i < Selection.gameObjects.Length; i++)
            if (Selection.gameObjects[i].GetComponentInChildren<MeshFilter>())
            {
                TargetMesh targetMesh = new TargetMesh(Selection.gameObjects[i].GetComponentInChildren<MeshFilter>());
                meshes.Add(targetMesh);
                totalVolume += targetMesh.volume;
            }
        meshes.OrderByDescending(mesh => mesh.ratio);

        // Set each meshe's ratio.
        for (int i = 0; i < meshes.Count; i++)
            meshes[i].ratio = meshes[i].volume / totalVolume;
    }

    [MenuItem("Window/Tools/Atlas Maker")]
    static void Open()
    {
        GetWindow(typeof(AtlasMaker)).titleContent = new GUIContent(typeof(AtlasMaker).Name);
    }

    #region Mesh volume.
    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1 / 6f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
    public static float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }
    #endregion
}
