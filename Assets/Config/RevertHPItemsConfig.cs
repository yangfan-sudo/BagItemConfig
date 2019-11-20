using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class RevertHPItemConfigData : BagItemConfigData
{
    [Tooltip("回复血量")]
    public int RevertHp;
    [Tooltip("持续时间")]
    public float Duration;
}

[CreateAssetMenu(menuName = "SolarLand/Item/回复道具", fileName = "reverthpitemsconfig")]
public class RevertHPItemsConfig : BagItemConfigs<RevertHPItemConfigData>
{    
    
    static RevertHPItemsConfig m_instance;

    public static RevertHPItemConfigData GetConfig(int id)
    {
        if (!Load())
            return null;

        if (id <= 0 || id > m_instance.ItemConfigs.Count)
        {
      
            return null;
        }

        return m_instance.ItemConfigs[id - 1];
    }

    public static bool Load()
    {
        if (m_instance != null)
        {
            return true;
        }
        return m_instance != null;
    }

    public static void Unload()
    {
        m_instance = null;
    }

   
}

