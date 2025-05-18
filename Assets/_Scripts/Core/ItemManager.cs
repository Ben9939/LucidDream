using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SceneMapManager;

/// <summary>
/// ItemManagerクラスは、プレイヤーの所持品を管理し、
/// アイテムの追加、削除、詳細情報の表示、ならびにアイテムの使用を処理します。
/// シングルトンパターンを採用しており、異なるシーン間でインスタンスを共有できます。
/// </summary>
public class ItemManager : MonoBehaviour
{
    [Header("Item Data Settings")]
    // アイテムデータは PossessedItemsSO によって管理され、現在の所持品が保存されます。
    [SerializeField] public OwnedItemsSO OwnedItems;

    [Header("UI Settings")]
    // 各アイテムに対応するボタン（各ボタンにはTextコンポーネントが付いている必要があります）
    [SerializeField] private List<GameObject> itemButtonList = new List<GameObject>();
    // アイテムデータの参照リスト
    [SerializeField] private List<ItemDataSO> itemDataList = new List<ItemDataSO>();
    // アイテムの詳細情報（説明文）を表示するためのText
    [SerializeField] private Text itemDetailText;
    // アイテムのアイコンを表示するためのImage
    [SerializeField] private Image itemIcon;
    // コレクションアイテム用のUIオブジェクト
    [SerializeField] private GameObject collectionItem;

    // シングルトンのインスタンス
    public static ItemManager Instance { get; private set; }
    // ゲームイベント ResetGame 発生時に、所持品のリセットを実行するアクション
    public UnityAction ResetItemAction;

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResetItemAction += ResetItem;
        GameEventManager.StartListening(GameEventName.ResetGame, ResetItemAction);

        // 初期状態として、preserveAspectをtrueに設定
        if (itemIcon != null)
        {
            itemIcon.preserveAspect = true;
        }

        // collectionItemにImageコンポーネントがあれば、preserveAspectをtrueに設定
        Image collectionImage = collectionItem.GetComponent<Image>();
        if (collectionImage != null)
        {
            collectionImage.preserveAspect = true;
        }

        // UIの初期化
        RefreshUI();
    }

    private void OnEnable()
    {
        // 有効化時に毎回ボタン表示を更新
        RefreshUI();
    }

    private void OnDisable()
    {
        // 詳細情報表示をクリア
        itemDetailText.text = "";
        // 前回の画像が残らないよう、次回の有効化前に画像もクリアする
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
        }
    }

    /// <summary>
    /// UIを更新し、所持品に基づいてボタンのテキストを更新します。
    /// まず全てのボタンのテキストをクリアし、所持品にある各アイテムの名称を順に設定します。
    /// </summary>
    private void RefreshUI()
    {
        itemIcon.sprite = null;
        itemDetailText.text = "";
        // 全てのボタンのテキストをクリア
        foreach (var button in itemButtonList)
        {
            button.GetComponent<Text>().text = "";
        }
        // 所持品にあるアイテムに応じてボタン表示を更新
        for (int i = 0; i < OwnedItems.Count && i < itemButtonList.Count; i++)
        {
            itemButtonList[i].GetComponent<Text>().text = OwnedItems[i].ItemName;
        }
    }

    /// <summary>
    /// 指定したインデックスのアイテムの詳細情報（アイコンと説明文）を表示します。
    /// </summary>
    /// <param name="index">所持品内のアイテムインデックス</param>
    public void ShowItemDetail(int index)
    {
        if (index >= 0 && index < OwnedItems.Count)
        {
            ItemDataSO data = OwnedItems[index];
            if (data != null)
            {
                itemIcon.sprite = data.Icon;
                // 画像切り替え時にpreserveAspectをtrueに設定
                itemIcon.preserveAspect = true;

                itemDetailText.text = data.Description;
            }
        }
        else
        {
            Debug.LogWarning("Invalid item index: " + index);
        }
    }

    /// <summary>
    /// 新しいアイテムを所持品に追加します。  
    /// アイテムが既に存在していてスタック可能な場合は数量を増加させ、  
    /// コレクションアイテムの場合は専用のUIを更新し、  
    /// それ以外は所持品の空きに応じてアイテムを追加し、ボタン表示を更新します。
    /// </summary>
    /// <param name="newItem">追加するアイテムのデータ</param>
    public void AddItem(ItemDataSO newItem)
    {
        if (newItem == null)
            return;

        // 所持品内に同じアイテムが既に存在するか（itemIDで判定）
        ItemDataSO existingItem = OwnedItems.Find(item => item.ItemID == newItem.ItemID);
        if (existingItem != null)
        {
            if (!newItem.IsStackable)
            {
                Debug.LogWarning($"Item {newItem.ItemName} already exists in possessions and is not stackable.");
                return;
            }
            else
            {
                // スタック可能な場合は数量を増加
                existingItem.Count++;
                Debug.Log($"Item {existingItem.ItemName} count increased to {existingItem.Count}.");
                RefreshUI();
                return;
            }
        }

        // コレクションアイテムの場合は専用UIのみ更新
        if (newItem.IsCollectionItem)
        {
            Image collectionImage = collectionItem.GetComponent<Image>();
            if (collectionImage != null)
            {
                collectionItem.SetActive(true);
                collectionImage.sprite = newItem.Icon;
                // preserveAspectをtrueに設定
                collectionImage.preserveAspect = true;
            }
            return;
        }

        // 通常のアイテムを追加（所持品に空きがある場合）
        if (OwnedItems.Count < itemButtonList.Count)
        {
            newItem.Count = 1;
            OwnedItems.Add(newItem);
            RefreshUI();
        }
        else
        {
            Debug.LogError("Item button list is full.");
        }
    }

    /// <summary>
    /// 所持品から指定されたアイテムを削除し、数量を更新します。  
    /// 数量が0以下になった場合はアイテムを削除し、UIを更新します。
    /// </summary>
    /// <param name="item">削除対象のアイテムデータ</param>
    public void RemoveItem(ItemDataSO item)
    {
        ItemDataSO existingItem = OwnedItems.Find(x => x.ItemID == item.ItemID);
        if (existingItem != null)
        {
            existingItem.Count--;
            Debug.Log($"Item {existingItem.ItemName} count decreased to {existingItem.Count}.");

            if (existingItem.Count <= 0)
            {
                int index = OwnedItems.IndexOf(existingItem);
                OwnedItems.RemoveAt(index);
                // 例：特定のアイテム（例："Candle"）使用時の追加処理
                if (item.ItemID == "Candle")
                    PlayerController.Instance.UsedCandle();
            }
            RefreshUI();
        }
    }

    /// <summary>
    /// 現在の在庫（OwnedItemsSOインスタンス）を返します。
    /// </summary>
    /// <returns>OwnedItemsSOインスタンス</returns>
    public OwnedItemsSO GetOwnedItems()
    {
        return OwnedItems;
    }

    /// <summary>
    /// 指定したターゲット上でアイテムを使用します。  
    /// アイテムがターゲットを必要とする場合、ターゲットが提供されないと警告を発します。  
    /// 使用後、アイテムの数量が尽きた場合は所持品から削除し、UIを更新します。
    /// </summary>
    /// <param name="target">アイテム使用対象のデータ</param>
    /// <param name="item">使用するアイテムのデータ</param>
    public void UseItemOnTarget(ObjectDataSO target, ItemDataSO item)
    {
        if (item != null && item.Ability != null)
        {
            if (item.Ability.RequiresTarget && target == null)
            {
                Debug.LogWarning($"Item {item.ItemName} requires a valid target but none was provided!");
                return;
            }
            item.Ability.Activate(target);
            RemoveItem(item);
        }
        else
        {
            Debug.LogWarning("Item or its ability is null.");
        }
    }

    /// <summary>
    /// 所持品に指定された itemID のアイテムが存在するか判定します。
    /// </summary>
    /// <param name="itemID">チェックするアイテムID</param>
    /// <returns>存在すれば true、存在しなければ false</returns>
    public bool HasItem(string itemID)
    {
        return OwnedItems.Exists(itemID);
    }

    /// <summary>
    /// 指定された itemID に基づいて所持品からアイテムデータを取得します。
    /// </summary>
    /// <param name="itemID">対象のアイテムID</param>
    /// <returns>存在すればアイテムデータ、存在しなければ null</returns>
    public ItemDataSO GetItem(string itemID)
    {
        return OwnedItems.Find(item => item.ItemID == itemID);
    }

    /// <summary>
    /// 現在の所持品（OwnedItemsSOインスタンス）を返します。
    /// </summary>
    /// <returns>OwnedItemsSOインスタンス</returns>
    public OwnedItemsSO GetPossessedItems()
    {
        return OwnedItems;
    }

    /// <summary>
    /// 所持品内の全アイテムをリセットします。  
    /// 各アイテムのデータをリセットし、詳細情報の表示をクリアするとともに、UIを更新します。
    /// </summary>
    public void ResetItem()
    {
        OwnedItems.ResetData();
        foreach (var item in itemDataList)
            item.ResetData();

        // 詳細情報と画像をクリア
        itemDetailText.text = "";
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
        }

        RefreshUI();
    }

    /// <summary>
    /// アプリケーション終了時に所持品をリセットし、イベントリスナーを解除します。
    /// </summary>
    private void OnApplicationQuit()
    {
        ResetItem();
        GameEventManager.StartListening(GameEventName.ResetGame, ResetItemAction);
        ResetItemAction -= ResetItem;
    }
}