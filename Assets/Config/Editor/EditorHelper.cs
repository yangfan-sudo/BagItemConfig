using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

public static class EditorHelper
{
    public static void ShowScriptInfo(this Editor editor, string title = "Script")
    {
        if (editor.target == null || editor.serializedObject == null)
            return;
        if (editor.serializedObject.isEditingMultipleObjects)
        {
            Type type = null;
            foreach (var obj in editor.serializedObject.targetObjects)
            {
                if (type == null)
                    type = obj.GetType();
                else if (type != obj.GetType())
                    return;
            }

            if (type == null)
                return;
            ShowScriptInfo(title, editor.target, editor.serializedObject.targetObjects[0]);
        }
        else
        {
            ShowScriptInfo(title, editor.target, editor.serializedObject.targetObject);
        }
    }

    public static void ShowInspectorScriptInfo(this Editor editor, string title = "Editor")
    {
        ShowScriptInfo(title, editor, editor);
    }

    public static void ShowInspectorTarget(this Editor editor, string title = "Target")
    {
        if (editor.target == null)
            return;
        GUI.enabled = false;
        EditorGUILayout.ObjectField(title, editor.target, editor.target.GetType(), false);
        GUI.enabled = true;
    }

    public static void DrawColorText(string content, Color color)
    {
        DrawColorContentFunc(content, color, EditorGUILayout.TextField, null, 0);
    }
    
    public static void DrawColorText(string content, Color color, float width)
    {
        DrawColorContentFunc(content, color, EditorGUILayout.TextField, null, width);
    }

    public static void DrawColorText(string content, Color color, Action<GUIStyle> styleHandler)
    {
        DrawColorContentFunc(content, color, EditorGUILayout.TextField, styleHandler, 0);
    }

    public static void DrawColorText(string content, Color color, float width, Action<GUIStyle> styleHandler)
    {
        DrawColorContentFunc(content, color, EditorGUILayout.TextField, styleHandler, width);
    }

    public static void DrawColorLabel(string content, Color color)
    {
        DrawColorContentFunc(content, color, (label,style,options)=>
        {EditorGUILayout.LabelField(label, style, options);return label; }, null, 0);
    }

    public static void DrawColorLabel(string content, Color color, float width)
    {
        DrawColorContentFunc(content, color, (label, style, options) =>
            { EditorGUILayout.LabelField(label, style, options); return label; }, null, width);
    }

    public static void DrawColorLabel(string content, Color color, Action<GUIStyle> styleHandler)
    {
        DrawColorContentFunc(content, color, (label, style, options) =>
            { EditorGUILayout.LabelField(label, style, options); return label; }, styleHandler, 0);
    }

    public static void DrawColorLabel(string content, Color color, float width, Action<GUIStyle> styleHandler)
    {
        DrawColorContentFunc(content, color, (label, style, options) =>
            { EditorGUILayout.LabelField(label, style, options); return label; }, styleHandler, width);
    }
    
    public static void DrawSplitLine()
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.grey);
    }

    public static void DrawSplitLine(Color color)
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), color);
    }

    public static void DrawSplitLine(float height)
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), Color.grey);
    }

    public static void DrawSplitLine(Color color,float height)
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);
    }

    private delegate string DrawColorContent(string label, GUIStyle style, params GUILayoutOption[] options);

    private static void DrawColorContentFunc(string content, Color color, DrawColorContent func, Action<GUIStyle> styleHandler, float width)
    {
        var elementLabelStyle = new GUIStyle(EditorStyles.label) {richText = true};
        styleHandler?.Invoke(elementLabelStyle);
        if (width <= 0)
            func(GetColorString(content, color), elementLabelStyle);
        else
            func(GetColorString(content, color), elementLabelStyle, GUILayout.Width(width));
    }

    private static void ShowScriptInfo(string title, Object target, Object targetObject)
    {
        if (target == null || targetObject == null)
            return;

        MonoScript monoScript = null;
        if (target is MonoBehaviour monoTarget)
        {
            monoScript = MonoScript.FromMonoBehaviour(monoTarget);
        }
        else if (target is ScriptableObject scriptObjTarget)
        {
            monoScript = MonoScript.FromScriptableObject(scriptObjTarget);
        }

        if (monoScript == null)
            return;

        GUI.enabled = false;
        EditorGUILayout.ObjectField(title, monoScript,targetObject.GetType(), false);
        GUI.enabled = true;

    }

    private static string GetColorString(string content, Color color)
    {
        var r = ((byte)Mathf.Clamp(Mathf.FloorToInt(color.r * 255), 0, 255)).ToString("X2");
        var g = ((byte)Mathf.Clamp(Mathf.FloorToInt(color.g * 255), 0, 255)).ToString("X2");
        var b = ((byte)Mathf.Clamp(Mathf.FloorToInt(color.b * 255), 0, 255)).ToString("X2");
        var a = ((byte)Mathf.Clamp(Mathf.FloorToInt(color.a * 255), 0, 255)).ToString("X2");
        return $"<color=#{r}{g}{b}{a}>{content}</color>";
    }

    [MenuItem("Assets/Solarland/extract particle mesh", false, 1)]
    static public void ExtractParticleMesh()
    {
        List<string> paths = GetSelectedAllPaths(".fbx", false);

        if (paths.Count > 0)
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Extract Particle Mesh", "", 0);
            for (int i = 0; i < paths.Count; ++i)
            {
                ExtractParticleMesh(paths[i]);
                UnityEditor.EditorUtility.DisplayProgressBar("Extract Particle Mesh", i + "/" + paths.Count, i / (float)paths.Count);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        }
        else
        {
            Debug.Log("ExtractParticleMesh: No *.fbx selected...");
        }
    }

    static public void ExtractParticleMesh(string assetPath)
    {
        UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        if (objs == null || objs.Length == 0)
            return;

        List<Mesh> meshList = new List<Mesh>();
        foreach (var obj in objs)
        {
            Mesh mesh = obj as Mesh;
            if (mesh == null)
                continue;
            meshList.Add(mesh);
        }

        for (int i = 0; i < meshList.Count; ++i)
        {
            string filename = Path.GetFileNameWithoutExtension(assetPath);
            if (meshList.Count > 1)
                filename = filename + "_" + i.ToString();

            string newPath = Path.GetDirectoryName(assetPath) + "/" + filename + ".asset";
            newPath = newPath.Replace("\\", "/");
            newPath = newPath.Replace("Temp/", "");

            Mesh clone = UnityEngine.Object.Instantiate(meshList[i]) as Mesh;
            if (clone == null)
            {
                return;
            }

            // optimize mesh
            clone.colors = null;
            clone.tangents = null;
            clone.uv2 = null;
            clone.uv3 = null;
            clone.uv4 = null;
            AssetDatabase.CreateAsset(clone, newPath);
        }
    }

    // 根据扩展名筛选文件 e.g. ".fbx", ".prefab", ".asset", "*.*"
    static private List<string> GetSelectedAllPaths(string extension, bool bConsiderFilter = false)
    {
        List<string> paths = new List<string>();
        bool bAll = string.Compare(extension, "*.*", System.StringComparison.OrdinalIgnoreCase) == 0 ? true : false;

        UnityEngine.Object[] objs = Selection.objects;
        foreach (UnityEngine.Object obj in objs)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (bConsiderFilter)
            {
                if (!AutoGenAssetBundleName.IsPassByWhiteList(path))
                    continue;
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] files = di.GetFiles(bAll ? extension : "*" + extension, SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; ++i)
                {
                    path = files[i].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");

                    if (!ValidExtension(path))      // 剔除meta，cs
                        continue;

                    if (bConsiderFilter && AutoGenAssetBundleName.IsBlockedByBlackList(path))
                        continue;

                    paths.Add(path);
                }
            }
            else
            {
                if (bAll || path.IndexOf(extension, System.StringComparison.OrdinalIgnoreCase) != -1)
                {
                    if (!ValidExtension(path))
                        continue;

                    if (bConsiderFilter && AutoGenAssetBundleName.IsBlockedByBlackList(path))
                        continue;

                    paths.Add(path);
                }
            }
        }

        return paths;
    }

    static public bool ValidExtension(string filePath)
    {
        if (filePath.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}
