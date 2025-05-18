using System;
using UnityEngine;

/// <summary>
/// シナリオの状態を管理するScriptableObjectです。  
/// ・Conditionプロパティで状態を参照でき、Activeメソッドで状態を反転させます。  
/// ・ResetDataで状態をfalseにリセットします。
/// </summary>
[CreateAssetMenu(menuName = "Scenario/ScenarioState")]
public class ScenarioStateSO : ScriptableObject
{
    [SerializeField] private bool condition = false;

    /// <summary>
    /// この状態の名前（ScriptableObjectの名前と同じ）を返します。
    /// </summary>
    public string StateName => this.name;

    /// <summary>
    /// 現在の状態（trueまたはfalse）を返します。
    /// </summary>
    public bool Condition { get => condition; }

    /// <summary>
    /// 状態を反転させます。  
    /// （trueの場合はfalseに、falseの場合はtrueに変更されます）
    /// </summary>
    public void Active()
    {
        condition = true;
    }

    /// <summary>
    /// 状態を初期状態（false）にリセットします。
    /// </summary>
    public void ResetData()
    {
        condition = false;
    }
}

//using System;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Scenario/ScenarioState")]
//public class ScenarioStateSO : ScriptableObject
//{
//    [SerializeField] private bool condition = false;
//    public string StateName => this.name;
//    public bool Condition { get => condition;}
//    public void Active()
//    {
//        condition = !condition;
//    }
//    public void ResetData()
//    {
//        condition = false;
//    }
//}
