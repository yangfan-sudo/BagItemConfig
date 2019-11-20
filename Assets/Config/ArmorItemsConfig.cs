using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ArmorItemConfigData : BagItemConfigData
{
    public int ArmorValue;
}

[CreateAssetMenu(menuName = "SolarLand/Item/护甲", fileName = "armoritemsconfig")]
public class ArmorItemsConfig : BagItemConfigs<ArmorItemConfigData>
{

    static ArmorItemsConfig m_instance;
  

    public static ArmorItemConfigData GetConfig(int id)
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

