using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// アイテム使用時のイベント処理を管理するScriptableObjectです。
/// ・獲得アイテムの追加、消費アイテムの削除、ユニットやシナリオ状態の更新を実施します。
/// </summary>
[CreateAssetMenu(menuName = "GameEvent/DialogEvent/UseItem")]
public class DialogUseItemEventSO : ScriptableObject
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
