using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 背包物品类型枚举
/// </summary>
public enum eBagItemType : byte
{
    /// <summary>
    /// 武器
    /// </summary>
    Weapon,

    /// <summary>
    /// 道具
    /// </summary>
    Prop,
    /// <summary>
    /// 回复道具
    /// </summary>
    Revert,
    /// <summary>
    /// 防具
    /// </summary>
    Armor
}

[System.Serializable]
public struct PathPair
{
    public string GUID;
    public string Folder;
    public string File;
    public string Name;
}
/// <summary>
/// 背包物品配置数据
/// </summary>
[System.Serializable]
public class BagItemConfigData
{
    [Tooltip("描述")]
    public string Desc;                                 // 描述

    [Tooltip("物品ID")]
    public byte ID;

    [Tooltip("物品类型")]
    public eBagItemType ItemType = eBagItemType.Revert;

    [Tooltip("物品重量")]
    public int ItemWeight;

    public PathPair PrefabPath;

}
public interface IBagItemListFunc
{
    void CheckIdLegal(ref int crossborder, ref int incontinuity);
    void SortList();
}
public abstract class BagItemConfigs<T>:ScriptableObject,IBagItemListFunc where T: BagItemConfigData
{
    public List<T> ItemConfigs;

    public virtual void CheckIdLegal(ref int crossborder, ref int incontinuity)
    {
        crossborder = -1;
        incontinuity = -1;
        if (ItemConfigs == null)
        {
            return;
        }
        ItemConfigs.Sort((l, r) => l.ID - r.ID);

        for (int i = 0; i < ItemConfigs.Count; ++i)
        {
            int id = ItemConfigs[i].ID;
            if (id <= 0 || id > ItemConfigs.Count)
            {
                crossborder = id;
                continue;
            }

            if ((id - 1) != i)
            {
                incontinuity = id;
                continue;
            }
        }
    }


    public virtual void SortList()
    {
        ItemConfigs.Sort((l, r) => l.ID - r.ID);
    }
}




