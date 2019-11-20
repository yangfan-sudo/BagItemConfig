using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

class AutoGenAssetBundleName : AssetPostprocessor
{
    // 路径白名单列表，仅对以下路径下的资源做处理
    static private string[] path_white_list = { "Assets/ResClient/", "Assets/ResServer/" };

    /// <summary>
    /// 路径是否在白名单内
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns>true：在白名单内</returns>
    static public bool IsPassByWhiteList(string assetPath)
    {
        for (int i = 0; i < path_white_list.Length; ++i)
        {
            if (assetPath.Contains(path_white_list[i]))
                return true;
        }

        return false;
    }

    // 文件夹黑名单列表，遍历文件夹时忽略 
    static private string[] folder_black_list = { "resources", "scenes", "scripts", "RawData", "Temp"};

    /// <summary>
    /// 文件夹是否在黑名单内
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns>true: 在黑名单内</returns>
    static public bool IsBlockedByBlackList(string assetPath)
    {
        string[] folderNames = assetPath.Split('/');
        foreach (string path in folder_black_list)
        {
            for (int i = 0; i < folderNames.Length; ++i)
            {
                if (string.Compare(path, folderNames[i], true) == 0)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查文件及文件夹名不能有空格 && 自动生成AB包名
    /// 仅对特定路径下文件进行处理
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    /// 添加操作：importedAssets包含所有子目录及文件，无序
    /// 删除操作：deletedAssets包含所有子目录下的文件及文件夹，无序
    /// 移动操作：movedAssets, movedFromAssetPaths从哪里移动至哪里
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //foreach (string str in importedAssets)
        //{
        //    Debug.Log("Reimported Asset: " + str);
        //}
        //foreach (string str in deletedAssets)
        //{
        //    Debug.Log("Deleted Asset: " + str);
        //}

        //for (int i = 0; i < movedAssets.Length; i++)
        //{
        //    Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        //}

        for (int i = 0; i < importedAssets.Length; ++i)
        {
            UpdateAssetBundleName(importedAssets[i]);
        }

        for (int i = 0; i < deletedAssets.Length; ++i)
        {
            UpdateAssetBundleName(deletedAssets[i]);
        }

        for (int i = 0; i < movedAssets.Length; ++i)
        {
            UpdateAssetBundleName(movedAssets[i]);
        }

        for (int i = 0; i < movedFromAssetPaths.Length; ++i)
        {
            UpdateAssetBundleName(movedFromAssetPaths[i]);
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    /// <summary>
    /// 更新文件夹的assetbundle name（文件并不生成ab name）
    /// AB名生成规则：文件夹下有任何一个文件即需要生成AB name，否则不生成
    /// </summary>
    /// <param name="assetPath">文件的完整路径</param>
    static void UpdateAssetBundleName(string assetPath)
    {
        // 屏蔽指定路径
        if (!IsPassByWhiteList(assetPath) || IsBlockedByBlackList(assetPath))
        {
            if (Path.GetExtension(assetPath) == ".cs")
                return;

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if(importer != null)
            {
                importer.assetBundleName = "";
            }

            return;
        }

        bool isDirectory = string.IsNullOrEmpty(Path.GetExtension(assetPath));              // 依据后缀名判定是否是文件夹
        if (isDirectory)
        {
            return;         // 无论增、删文件夹都不处理
        }

        string directory = assetPath.Substring(0, assetPath.LastIndexOf("/"));              // 找到文件所在的文件夹

        AssetImporter ti = AssetImporter.GetAtPath(directory);
        if (ti == null)
            return;     // 文件夹可能不存在

        int count = 0;          // 统计非meta文件数量
        string[] files = Directory.GetFiles(directory);
        foreach (string file in files)
        {
            if (file.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            ++count;
        }

        // meta不能算是文件
        if (count == 0)
        {
            ti.assetBundleName = "";
        }
        else
        {
            ti.assetBundleName = directory.ToLower().Replace("assets/resclient/", "") + ".ab";
        }
    }
}
