using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有しているアイテムを管理するScriptableObjectです。
/// IEnumerable&lt;ItemDataSO&gt;を実装しており、foreachでの反復処理が可能です。
/// </summary>
[CreateAssetMenu(menuName = "Items/OwnedItems")]
public class OwnedItemsSO : ScriptableObject, IEnumerable<ItemDataSO>
{
    // 所有アイテムのリスト（プライベート、camelCase）
    [SerializeField]
    private List<ItemDataSO> ownedItems = new List<ItemDataSO>();

    /// <summary>
    /// 現在の所持アイテム数を返します。
    /// </summary>
    public int Count => ownedItems.Count;

    /// <summary>
    /// インデクサ：ownedItemsSO[index]の形式でアイテムにアクセスできます。
    /// </summary>
    public ItemDataSO this[int index]
    {
        get => ownedItems[index];
        set => ownedItems[index] = value;
    }

    /// <summary>
    /// 指定されたアイテムのインデックスを返します。
    /// </summary>
    public int IndexOf(ItemDataSO item)
    {
        return ownedItems.IndexOf(item);
    }

    /// <summary>
    /// 新しいアイテムを所有リストに追加します。
    /// </summary>
    public void Add(ItemDataSO newItem)
    {
        ownedItems.Add(newItem);
    }

    /// <summary>
    /// 指定したインデックスのアイテムをリストから削除します。
    /// </summary>
    public void RemoveAt(int index)
    {
        ownedItems.RemoveAt(index);
    }

    /// <summary>
    /// 条件に一致するアイテムをリストから検索して返します。
    /// </summary>
    public ItemDataSO Find(Predicate<ItemDataSO> predicate)
    {
        return ownedItems.Find(predicate);
    }

    /// <summary>
    /// 指定したitemIDを持つアイテムが存在するかを返します。
    /// </summary>
    public bool Exists(string itemID)
    {
        return ownedItems.Exists(item => item.ItemID == itemID);
    }

    /// <summary>
    /// IEnumerable&lt;ItemDataSO&gt;の実装により、foreachで反復処理が可能です。
    /// </summary>
    public IEnumerator<ItemDataSO> GetEnumerator()
    {
        return ownedItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 所有アイテムのデータをリセットし、リストを空にします。
    /// </summary>
    public void ResetData()
    {
        ownedItems.Clear();
    }
    /// <summary>
    /// 所有アイテムから、Puzzle としてマークされたアイテムのパズルプレハブ一覧を取得します。
    /// </summary>
    /// <returns>パズルプレハブのリスト</returns>
    public List<GameObject> GetPuzzlePrefabs()
    {
        List<GameObject> puzzlePrefabs = new List<GameObject>();
        foreach (ItemDataSO item in ownedItems)
        {
            if (item.IsPuzzle && item.PuzzlePrefab != null)
            {
                puzzlePrefabs.Add(item.PuzzlePrefab);
            }
        }
        return puzzlePrefabs;
    }
}