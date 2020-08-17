using System.IO;
using UnityEditor;
using UnityEngine;

public class DataObject : ScriptableObject
{
    public string path;

    public static T Get<T>(ref T dataHolder, string folderPath) where T : DataObject
    {
        // Return the dataHolder.
        if (dataHolder)
            return dataHolder;

        // DataHolder is empty, try to load a save file.
        string filePath = "Assets" + folderPath +"/" + typeof(T) + ".asset";
        dataHolder = AssetDatabase.LoadAssetAtPath(filePath, typeof(T)) as T;
        if (dataHolder)
            return dataHolder;

        // No save file exists, create one.
        if (!Directory.Exists(Application.dataPath + folderPath + "/"))
            Directory.CreateDirectory(Application.dataPath + folderPath + "/");
        dataHolder = CreateInstance(typeof(T)) as T;
        dataHolder.path = filePath;
        Path.GetDirectoryName(filePath);
        File.WriteAllText(filePath, "");
        AssetDatabase.CreateAsset(dataHolder, filePath);
        GUI.changed = true;

        return dataHolder;
    }

    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
}
