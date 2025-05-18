using UnityEngine;

/// <summary>
/// アイテムのデータを管理するScriptableObjectです。
/// ・基本情報、拡張情報、能力、カウント情報などを保持し、必要に応じて状態と連動します。
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("基本資料")]
    [Tooltip("アイテムの一意な識別子（例：Key, Candleなど）")]
    public string ItemID;

    [Tooltip("アイテムの名称")]
    public string ItemName;

    [TextArea, Tooltip("アイテムの詳細な説明")]
    public string Description;

    [Tooltip("アイテムのアイコン")]
    public Sprite Icon;

    [Header("物品拡張")]
    [Tooltip("パズルアイテムかどうか")]
    public bool IsPuzzle;

    [Tooltip("パズルアイテムの場合に使用するプレハブ")]
    public GameObject PuzzlePrefab;

    [Header("物品能力")]
    [Tooltip("アイテムの能力（例：KeyAbility、CandleAbilityなど）")]
    public ItemAbilitySO Ability = null;

    [Header("計数情報")]
    [Tooltip("アイテムの状態（例：hasKey）と連動し、数量更新時に自動同期します")]
    public UnitStateSO ItemCountState;

    [Header("追加設定")]
    [Tooltip("アイテムが重複取得可能かどうか（スタック可能な場合true）")]
    public bool IsStackable = true;

    [Tooltip("コレクションアイテムかどうか")]
    public bool IsCollectionItem = false;

    // 内部用：アイテムの所持数
    [SerializeField]
    private int count = 0;

    /// <summary>
    /// アイテムの所持数。値が変わるとItemCountStateが自動更新されます。
    /// </summary>
    public int Count
    {
        get { return count; }
        set
        {
            if (count != value)
            {
                count = value;
                if (ItemCountState != null)
                {
                    ItemCountState.Value = count;
                }
            }
        }
    }

    /// <summary>
    /// エディタ上でCountが変更されたときにItemCountStateを更新します。
    /// </summary>
    private void OnValidate()
    {
        if (ItemCountState != null)
        {
            ItemCountState.Value = count;
        }
    }

    /// <summary>
    /// アイテムのデータをリセットし、所持数を0にします。
    /// </summary>
    public void ResetData()
    {
        count = 0;
        ItemCountState?.ResetData();
    }
}

//using UnityEngine;

//[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Data")]
//public class ItemDataSO : ScriptableObject
//{
//    [Header("基本資料")]
//    public string itemID;       // 唯一標識，例如 "Key", "Candle" 等
//    public string itemName;     // 物品名稱
//    [TextArea]
//    public string description;  // 物品描述
//    public Sprite icon;         // 物品圖示

//    [Header("物品擴展")]
//    public bool isPuzzle;           // 是否為 Puzzle 物品
//    public GameObject puzzlePrefab; // 若是 Puzzle，對應的預製件

//    [Header("物品能力")]
//    public ItemAbilitySO ability = null; // 物品作用模塊

//    [Header("計數資訊")]
//    [Tooltip("關聯的物品狀態，例如 hasKey，當數量更新時自動同步更新該狀態")]
//    public UnitStateSO itemCountState;

//    [Header("額外設定")]
//    [Tooltip("是否可以重複取得該物品（例如：可堆疊物品）")]
//    public bool isStackable = true;

//    public bool isCollectionItem = false;

//    // 使用一個序列化的私有欄位作為後備
//    [SerializeField]
//    private int count = 0;
//    public int Count
//    {
//        get { return count; }
//        set
//        {
//            if (count != value)
//            {
//                count = value;
//                // 當 count 改變時，自動更新 itemCountState
//                if (itemCountState != null)
//                {
//                    itemCountState.Value = count;
//                }
//            }
//        }
//    }

//    // 可選：在編輯器中修改 count 時也能自動更新 itemCountState
//    private void OnValidate()
//    {
//        if (itemCountState != null)
//        {
//            itemCountState.Value = count;
//        }
//    }
//    public void ResetData()
//    {
//        count = 0;
//    }
//}

//using UnityEngine;

//[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Data")]
//public class ItemDataSO : ScriptableObject
//{
//    [Header("基本資料")]
//    public string itemID;       // 唯一標識，例如 "Key", "Candle" 等
//    public string itemName;     // 物品名稱
//    [TextArea]
//    public string description;  // 物品描述
//    public Sprite icon;         // 物品圖示

//    [Header("物品擴展")]
//    public bool isPuzzle;           // 是否為 Puzzle 物品
//    public GameObject puzzlePrefab; // 若是 Puzzle，對應的預製件（必須配置）

//    [Header("物品能力")]
//    public ItemAbilitySO ability = null; // 物品作用模塊（例如 KeyAbility、CandleAbility 等）

//    [Header("獲得物品時觸發的事件")]
//    public ItemAcquisitionEventSO acquisitionEvent = null;


//}


//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class ItemData : MonoBehaviour
//{
//    public ItemTypes type;
//    public string description;
//    public string itemName;
//    [HideInInspector] public Sprite icon;

//    private void Start()
//    {
//        // 假設該物件有 SpriteRenderer 組件，初始化 icon
//        SpriteRenderer sr = GetComponent<SpriteRenderer>();
//        if (sr != null)
//        {
//            icon = sr.sprite;
//        }
//    }

//    // 返回所有 Puzzle 類型的道具（示例用）
//    public static ItemTypes[] GetAllPuzzles()
//    {
//        return new ItemTypes[]
//        {
//            ItemTypes.Puzzle1,
//            ItemTypes.Puzzle2,
//            ItemTypes.Puzzle3
//        };
//    }
//}

//public enum ItemTypes
//{
//    None,
//    Candle,
//    Key,
//    Puzzle1,
//    Puzzle2,
//    Puzzle3
//}
