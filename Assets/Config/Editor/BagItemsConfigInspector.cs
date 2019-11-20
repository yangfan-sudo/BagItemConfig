using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(PathPair), true)]
public class PrefabPathInspector : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {

        var GUIDProp = property.FindPropertyRelative(AssetPathHelper.GUIDPropName);
        var FolderProp = property.FindPropertyRelative(AssetPathHelper.FolderPropName);
        var FileProp = property.FindPropertyRelative(AssetPathHelper.FilePropName);
        var NameProp = property.FindPropertyRelative(AssetPathHelper.NamePropName);

        AssetPathHelper.TranslatePathPairToAssetsPath(GUIDProp, FolderProp, FileProp,
            out var assetPath, out var file, out var guidPath, out var guidFile);
        Object obj = null;

        if (File.Exists(file))
        {
            obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
        }
        else if (File.Exists(guidFile))
        {
            obj = AssetDatabase.LoadAssetAtPath(guidPath, typeof(GameObject));
        }
        obj = EditorGUI.ObjectField(rect, label, obj, typeof(GameObject), false);
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
            GUIDProp.stringValue = FolderProp.stringValue = FileProp.stringValue = "";
        }
    }
}
[CustomEditor(typeof(BagItemConfigs<>), true)]
public class BagItemsConfigInspector : Editor
{
    public override void OnInspectorGUI()
    {
        this.ShowInspectorTarget();
        this.ShowInspectorScriptInfo();
        base.OnInspectorGUI();

        if (GUILayout.Button("根据id排序"))
        {
            (target as IBagItemListFunc).SortList();
        }

        if (GUILayout.Button("检查id是否合法（越界、连续）"))
        {
            IBagItemListFunc func = target as IBagItemListFunc;
            int crossborderid = -1;
            int incontinuityid = -1;
            func.CheckIdLegal(ref crossborderid, ref incontinuityid);

            if (crossborderid > -1)
            {
                EditorUtility.DisplayDialog("错误", string.Format("id: {0} 越界", crossborderid), "OK");
            }

            if (incontinuityid > -1)
            {
                EditorUtility.DisplayDialog("错误", string.Format("id: {0} 不连续", incontinuityid), "OK");
            }
            if (incontinuityid == -1 && crossborderid == -1)
            {
                EditorUtility.DisplayDialog("", "检查通过，完美！", "OK");
            }
        }
    }
}

#endif