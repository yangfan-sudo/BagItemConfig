using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public static class AssetPathHelper
{
    public const string GUIDPropName = "GUID";
    public const string FolderPropName = "Folder";
    public const string FilePropName = "File";
    public const string NamePropName = "Name";
    private static readonly Type pathPairType = typeof(PathPair);

    public static void AddAssetChangedCheckHandler<T>(Func<SerializedObject,Func<SerializedObject, SerializedProperty, bool>, bool> checker) 
        where T : ScriptableObject
    {
        bool CheckPath(SerializedObject serializedObject, SerializedProperty pathPairProp)
        {
            CheckPathPairType(pathPairProp.type, "SerializedProperty");

            var objectAssetPath = AssetDatabase.GetAssetPath(serializedObject.targetObject);

            bool isChanged = false;
            var GUIDProp = pathPairProp.FindPropertyRelative(GUIDPropName);
            var FolderProp = pathPairProp.FindPropertyRelative(FolderPropName);
            var FileProp = pathPairProp.FindPropertyRelative(FilePropName);
            var NameProp = pathPairProp.FindPropertyRelative(NamePropName);

            if (GUIDProp.stringValue == string.Empty)
            {
                if (FolderProp.stringValue == string.Empty && FileProp.stringValue == string.Empty)
                {
                    return false;
                }  
                FolderProp.stringValue = FileProp.stringValue = NameProp.stringValue = "";
                return true;
            }
            var assetPath = AssetDatabase.GUIDToAssetPath(GUIDProp.stringValue);
            if (assetPath == "") // 清除失效文件
            {
                Debug.LogError($"配置文件 {objectAssetPath} 中的引用{FolderProp.stringValue}/{FileProp.stringValue} 失效！");
                GUIDProp.stringValue = FolderProp.stringValue = FileProp.stringValue = NameProp.stringValue = "";
                return true;
            }
            var tmpPath = assetPath.Replace("Assets/ResClient/", "");
            var index = tmpPath.LastIndexOf('/');
            if (index < 0)
            {
                Debug.LogError($"配置文件 {objectAssetPath} 中的引用{FolderProp.stringValue}/{FileProp.stringValue} 失效！");
                GUIDProp.stringValue = FolderProp.stringValue = FileProp.stringValue = "";
                return true;
            } 
            var folder = tmpPath.Substring(0, index) + ".ab";
            var file = Path.GetFileName(assetPath);

            if (folder != FolderProp.stringValue)
            {
                FolderProp.stringValue = folder.ToLower();
                isChanged = true;
            }

            if (file != FileProp.stringValue)
            {
                FileProp.stringValue = file.ToLower();
                isChanged = true;
            }

            return isChanged;
        }

        void Handler()
        {
            AssetDatabase.Refresh();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] {"Assets/ResClient"});
            bool isChanged = false;
            foreach (var guid in guids)
            {
                var config = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                var serializedObject = new SerializedObject(config);
                serializedObject.Update();
                bool isObjectChanged = checker(serializedObject, CheckPath);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                isChanged = isObjectChanged || isChanged;
            }

            if (!isChanged) return;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorApplication.projectChanged += Handler;
    }

    public static void DrawPathPairAsset(SerializedProperty pathPairProp, Type assetType)
    {
        DrawPathPairAsset<Object>(pathPairProp, assetType);
    }
    
    public static T DrawPathPairAsset<T>(SerializedProperty pathPairProp) where T:Object
    {
        return DrawPathPairAsset<T>(pathPairProp, typeof(T));
    }

    private static T DrawPathPairAsset<T>(SerializedProperty pathPairProp, Type assetType) where T : Object
    {
        if (assetType == null || !typeof(Object).IsAssignableFrom(assetType))
            return default;

        CheckPathPairType(pathPairProp.type, "SerializedProperty");
        var GUIDProp = pathPairProp.FindPropertyRelative(GUIDPropName);
        var FolderProp = pathPairProp.FindPropertyRelative(FolderPropName);
        var FileProp = pathPairProp.FindPropertyRelative(FilePropName);
        var NameProp = pathPairProp.FindPropertyRelative(NamePropName);

        TranslatePathPairToAssetsPath(GUIDProp, FolderProp, FileProp,
            out var assetPath, out var file, out var guidPath, out var guidFile);

        Object obj = null;

        if (File.Exists(file))
        {
            obj = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
        }
        else if (File.Exists(guidFile))
        {
            obj = AssetDatabase.LoadAssetAtPath(guidPath, assetType);
        }

        obj = EditorGUILayout.ObjectField(obj, assetType, false);

        if (obj != null)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (path.Contains("Assets/ResClient/"))
            {
                var tmpPath = path.Replace("Assets/ResClient/", "");
                GUIDProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                FolderProp.stringValue = (tmpPath.Substring(0, tmpPath.LastIndexOf('/')) + ".ab").ToLower();
                FileProp.stringValue = Path.GetFileName(path).ToLower();
                NameProp.stringValue = Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "目前资源加载只支持Assets/ResClient/目录", "知道了");
            }
        }
        else
        {
            GUIDProp.stringValue = FolderProp.stringValue = FileProp.stringValue = NameProp.stringValue = "";
        }

        return obj as T;
    }

    private static void CheckPathPairType(string checkTypeName,string checkTypeInfo)
    {
        if (checkTypeName != pathPairType.Name)
        {
            
        }
    }
    public static void TranslatePathPairToAssetsPath(SerializedProperty GUIDProp, SerializedProperty FolderProp, 
        SerializedProperty FileProp,out string assetPath, out string file, out string guidPath, out string guidFile)
    {
        var guid = GUIDProp.stringValue;
        var folder = FolderProp.stringValue;
        var name = FileProp.stringValue;

        assetPath = Path.Combine("Assets/Resclient/", folder.Replace(".ab", ""), name);
        file = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
        guidPath = guid == "" ? "" : AssetDatabase.GUIDToAssetPath(guid);
        guidFile = Path.Combine(Application.dataPath, guidPath.Replace("Assets/", ""));
    }
}
