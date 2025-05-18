using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームオブジェクト（NPC以外）の状態を管理するためのScriptableObjectです。
/// UnitDataSOを継承し、状態の取得と更新を行います。
/// </summary>
[CreateAssetMenu(menuName = "Unit/ObjectData")]
public class ObjectDataSO : UnitDataSO
{
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

//[CreateAssetMenu(menuName = "Unit/ObjectData")]
//public class ObjectDataSO : UnitDataSO
//{
//    public int GetStateValue(string stateName)
//    {
//        UnitStateSO state = States.Find(s => s.StateName == stateName);
//        return state.Value;
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
