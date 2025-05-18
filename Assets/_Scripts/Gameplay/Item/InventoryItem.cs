using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// InventoryItem は、物品としてのアイテムの機能を実行するためのコンポーネントです。
/// </summary>
public class InventoryItem : MonoBehaviour
{
    [Header("Item Data (ScriptableObject)")]
    public ItemDataSO itemData;

    /// <summary>
    /// アイテムの初期化処理を行います
    /// </summary>
    private void InitializeItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("InventoryItem: itemData が指定されていません！");
            return;
        }
    }

    /// <summary>
    /// アイテムが使用された際、対象オブジェクトに対してアイテム効果を発動します。
    /// </summary>
    /// <param name="target">アイテム効果の対象オブジェクト</param>
    public void ActivateItem(ObjectDataSO target)
    {
        if (itemData == null)
        {
            Debug.LogWarning("InventoryItem: itemData が null のためアイテム効果を実行できません。");
            return;
        }
        if (itemData.Ability != null)
        {
            // ScriptableObject に定義された能力を実行
            itemData.Ability.Activate(target);
        }
        else
        {
            Debug.LogWarning("InventoryItem: " + itemData.ItemName + " に指定された能力がありません。");
        }
    }
}