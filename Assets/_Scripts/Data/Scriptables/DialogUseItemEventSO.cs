using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// アイテム使用時のイベント処理を管理するScriptableObjectです。
/// ・獲得アイテムの追加、消費アイテムの削除、ユニットやシナリオ状態の更新を実施します。
/// </summary>
[CreateAssetMenu(menuName = "GameEvent/DialogEvent/UseItem")]
public class DialogGetItemEventSO : ScriptableObject
{
    [Tooltip("獲得したアイテムのリスト")]
    public List<ItemDataSO> AcquiredItems;

    [Tooltip("消費するアイテムのリスト")]
    public List<ItemDataSO> ConsumedItems;

    [Tooltip("変更対象のユニット状態")]
    public UnitStateSO UnitStateToModify;

    [Tooltip("変更対象のシナリオ状態")]
    public ScenarioStateSO ScenarioStateToModify;

    [Tooltip("状態変更に用いる数値")]
    public int StateModificationValue;

    /// <summary>
    /// アイテムの追加／削除と状態の更新処理を実行します。
    /// </summary>
    public void HandleSelection()
    {
        // 獲得アイテムの追加
        if (AcquiredItems != null)
        {
            foreach (var item in AcquiredItems)
            {
                ItemManager.Instance.AddItem(item);
            }
        }

        // 消費アイテムの削除
        if (ConsumedItems != null)
        {
            foreach (var item in ConsumedItems)
            {
                ItemManager.Instance.RemoveItem(item);
            }
        }

        // ユニット状態の更新（数値加算）
        if (UnitStateToModify != null)
        {
            UnitStateToModify.Value += StateModificationValue;
        }

        // シナリオ状態の更新（アクティブ化）
        if (ScenarioStateToModify != null)
        {
            ScenarioStateToModify.Active();
        }
    }
}

//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//[CreateAssetMenu(menuName = "GameEvent/DialogEvent/UseItem")]
//public class DialogUseItemEventSO : ScriptableObject
//{
//    public List<ItemDataSO> acquiredItem;  // 獲得的物品
//    public List<ItemDataSO> consumedItem;  // 消耗的物品
//    public UnitStateSO UnitStateToModify; // 需要修改的狀態
//    public ScenarioStateSO ScenarioStateToModify; // 需要修改的狀態
//    public int stateModificationValue; // 修改的值

//    public void HandleSelection()
//    {

//        //獲得物品
//        if (acquiredItem != null)
//        {
//            foreach (var item in acquiredItem)
//            {
//                ItemManager.Instance.AddItem(item);
//            }
//        }

//        // 消耗物品
//        if (consumedItem != null)
//        {
//            foreach(var item in consumedItem)
//            {
//                ItemManager.Instance.RemoveItem(item);
//            }

//        }

//        // 更新狀態值
//        if (UnitStateToModify != null)
//        {
//            UnitStateToModify.Value += stateModificationValue;
//        }
//        if (ScenarioStateToModify != null)
//        {
//            ScenarioStateToModify.Active();
//        }
//    }
//}
