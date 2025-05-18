using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPCのデータを管理するScriptableObjectです。
/// UnitDataSOを継承し、対話用の立ち絵情報なども保持します。
/// </summary>
[CreateAssetMenu(menuName = "Unit/NPC/NpcData")]
public class NpcDataSO : UnitDataSO, IPortraitHolder
{
    [Header("立ち絵資料")]
    [Tooltip("NPCの立ち絵プレハブ")]
    public GameObject PortraitPrefabObject;

    /// <summary>
    /// IPortraitHolderインターフェース実装：NPCの立ち絵プレハブを返します。
    /// </summary>
    public GameObject PortraitPrefab => PortraitPrefabObject;

    /// <summary>
    /// 指定された状態名に対応する値を返します。
    /// </summary>
    public int GetStateValue(string stateName)
    {
        UnitStateSO state = States.Find(s => s.StateName == stateName);
        return state != null ? state.Value : 0;
    }

    /// <summary>
    /// 指定された状態名に対して値を設定します。
    /// </summary>
    public void SetStateValue(string stateName, int value)
    {
        UnitStateSO state = States.Find(s => s.StateName == stateName);
        if (state != null)
        {
            state.Value = value;
        }
    }
}

//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Unit/NPC/NpcData")]
//public class NpcDataSO : UnitDataSO, IPortraitHolder
//{
//    [Header("立繪資料")]
//    public GameObject portraitPrefab;

//    public GameObject PortraitPrefab => portraitPrefab;
//    public int GetStateValue(string stateName)
//    {
//        UnitStateSO state = States.Find(s => s.StateName == stateName);
//        return state != null ? state.Value : 0;
//    }

//    public void SetStateValue(string stateName, int value)
//    {
//        UnitStateSO state = States.Find(s => s.StateName == stateName);
//        if (state != null)
//        {
//            state.Value = value;
//        }
//    }
//}

////using System.Collections.Generic;
////using UnityEngine;

////[CreateAssetMenu(menuName = "Unit/NPC/NpcData")]
////public class NpcDataSO : UnitDataSO
////{
////    public List<Sprite> npcImage;
////    //public string unitID; 
////    //public string unitName; 
////    //public List<Sprite> npcImage;
////    //public List<DialogStateGraph> dialogStateGraphs;
////    //[HideInInspector] public DialogStateGraph currentDialogStateGraph;
////    //[HideInInspector] public DialogGraph currentDialogGraph;
////    //public List<UnitStateSO> states = new List<UnitStateSO>();
////    public int GetStateValue(string stateName)
////    {
////        UnitStateSO state = states.Find(s => s.StateName == stateName);
////        return state.Value;
////    }

////    public void SetStateValue(string stateName, int value)
////    {
////        UnitStateSO state = states.Find(s => s.StateName == stateName);
////        if (state != null)
////        {
////            state.Value = value;
////        }
////    }
////    //[SerializeField] private List<NpcVariable> npcVariables = new List<NpcVariable>(); // Inspector 可編輯的變數

////    //public Dictionary<string, int> GetInitialVariables()
////    //{
////    //    Dictionary<string, int> initialVariables = new Dictionary<string, int>();
////    //    foreach (var variable in npcVariables)
////    //    {
////    //        initialVariables[variable.key] = variable.value;
////    //    }
////    //    return initialVariables;
////    //}
////}

////[System.Serializable]
////public class NpcVariable
////{
////    public string key; 
////    public int value;  
////}
