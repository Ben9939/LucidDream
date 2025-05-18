using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 対話状態のエントリを表すクラスです。  
/// ・requiredStateが満たされる場合に、dialogStateGraphが有効となり、priorityで優先順位が決定されます。  
/// ・ResetDataで内部状態をリセットします。
/// </summary>
[System.Serializable]
public class DialogStateEntry
{
    [Tooltip("このエントリの条件となるシナリオ状態（nullの場合は無条件）")]
    public ScenarioStateSO requiredState;

    [Tooltip("条件が満たされた場合に適用する対話状態グラフ")]
    public DialogStateGraph dialogStateGraph;

    [Tooltip("優先度（数値が大きいほど優先されます）")]
    public int priority;

    /// <summary>
    /// 内部のrequiredStateおよびdialogStateGraphのデータをリセットします。
    /// </summary>
    public void ResetData()
    {
        requiredState?.ResetData();
        dialogStateGraph?.ResetData();
    }
}


/// <summary>
/// ユニット（プレイヤー、NPC、オブジェクトなど）の共通データを管理する抽象クラスです。  
/// ・ユニットのID、名称、対話状態グラフ、および状態リストを保持します。  
/// ・ResetDataで実行時のデータを初期状態に戻します。
/// </summary>
public abstract class UnitDataSO : ScriptableObject
{
    [Header("基本情報")]
    public string unitID;
    public string unitName;

    [SerializeField]
    private List<DialogStateEntry> dialogStateEntries = new List<DialogStateEntry>();

    [SerializeField, HideInInspector]
    private DialogStateGraph currentDialogStateGraph = null;

    [SerializeField]
    public DialogGraph currentDialogGraph = null;

    [SerializeField]
    private List<UnitStateSO> states = new List<UnitStateSO>();

    /// <summary>
    /// 現在有効な対話状態グラフを返します（条件および優先度に基づく）。
    /// </summary>
    public DialogStateGraph CurrentDialogStateGraph
    {
        get { return GetDialogStateGraph(); }
    }

    /// <summary>
    /// 指定されたシナリオ状態に対応する対話状態グラフを返します。
    /// </summary>
    public DialogStateGraph GetDialogStateGraph(ScenarioStateSO state)
    {
        foreach (var entry in dialogStateEntries)
        {
            if (entry.requiredState == state)
            {
                return entry.dialogStateGraph;
            }
        }
        return null;
    }

    /// <summary>
    /// 各エントリの条件と優先度に基づいて、最も優先度の高い対話状態グラフを返します。
    /// </summary>
    public DialogStateGraph GetDialogStateGraph()
    {
        int bestPriority = int.MinValue;
        DialogStateGraph bestStateGraph = null;
        foreach (var entry in dialogStateEntries)
        {
            bool conditionMet = (entry.requiredState == null) || entry.requiredState.Condition;
            if (conditionMet && entry.priority > bestPriority)
            {
                bestPriority = entry.priority;
                bestStateGraph = entry.dialogStateGraph;
            }
        }
        return bestStateGraph;
    }

    /// <summary>
    /// 現在の対話グラフを取得または設定します。
    /// </summary>
    public DialogGraph CurrentDialogGraph
    {
        get { return currentDialogGraph; }
        set { currentDialogGraph = value; }
    }

    /// <summary>
    /// ユニットの状態リストを返します。
    /// </summary>
    public List<UnitStateSO> States
    {
        get { return states; }
    }

    /// <summary>
    /// 実行時のデータを初期状態にリセットします。  
    /// ・対話グラフ、状態リスト、及び各エントリの内部データをリセットします。
    /// </summary>
    public virtual void ResetData()
    {
        currentDialogGraph = null;
        currentDialogStateGraph = null;
        foreach (var state in states)
            state?.ResetState();
        foreach (var entry in dialogStateEntries)
            entry?.ResetData();
    }
}

//using System.Collections.Generic;
//using UnityEngine;
//using XNode;


//[System.Serializable]
//public class DialogStateEntry
//{
//    public ScenarioStateSO requiredState;
//    public DialogStateGraph dialogStateGraph;
//    public int priority;

//    public void ResetData()
//    {
//        requiredState?.ResetData();
//        dialogStateGraph?.ResetData();
//    }
//}

//public abstract class UnitDataSO : ScriptableObject
//{
//    public string unitID;
//    public string unitName;
//    [SerializeField]
//    private List<DialogStateEntry> dialogStateEntries = new List<DialogStateEntry>();

//    [SerializeField, HideInInspector] private DialogStateGraph currentDialogStateGraph = null;
//    [SerializeField] public DialogGraph currentDialogGraph = null;
//    //[SerializeField, HideInInspector] private DialogGraph currentDialogGraph = null;
//    [SerializeField] private List<UnitStateSO> states = new List<UnitStateSO>();

//    public DialogStateGraph CurrentDialogStateGraph
//    {
//        get { return GetDialogStateGraph(); }
//    }
//    public DialogStateGraph GetDialogStateGraph(ScenarioStateSO state)
//    {
//        foreach (var entry in dialogStateEntries)
//        {
//            if (entry.requiredState == state)
//            {
//                return entry.dialogStateGraph;
//            }
//        }
//        return null;
//    }

//    public DialogStateGraph GetDialogStateGraph()
//    {
//        int bestPriority = int.MinValue;
//        DialogStateGraph bestStateGraph = null;
//        foreach (var entry in dialogStateEntries)
//        {
//            bool conditionMet = false;
//            if (entry.requiredState == null)
//            {
//                conditionMet = true;
//            }
//            else
//            {
//                if (entry.requiredState.Condition)
//                {
//                    conditionMet = true;
//                }
//            }
//            if (conditionMet && entry.priority > bestPriority)
//            {
//                bestPriority = entry.priority;
//                bestStateGraph = entry.dialogStateGraph;
//            }
//        }
//        return bestStateGraph;
//    }



//    public DialogGraph CurrentDialogGraph
//    {
//        get { return currentDialogGraph; }
//        set { currentDialogGraph = value; }
//    }

//    public List<UnitStateSO> States
//    {
//        get { return states; }
//    }

//    /// <summary>
//    /// 重置運行時的資料到預設狀態
//    /// </summary>
//    public virtual void ResetData()
//    {
//        currentDialogGraph = null;
//        currentDialogStateGraph = null;
//        foreach (var state in states)
//            state?.ResetState();
//        foreach (var entry in dialogStateEntries)
//            entry?.ResetData();
//    }
//}
