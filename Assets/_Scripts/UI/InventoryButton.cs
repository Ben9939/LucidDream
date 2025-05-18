using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// インベントリ内のアイテムボタン処理を管理するクラス
/// </summary>
public class InventoryButton : MonoBehaviour
{
    /// <summary>
    /// ボタンの名前に応じたアイテム詳細表示を行う
    /// </summary>
    void Start()
    {
        string buttonName = gameObject.name;
        GetComponent<Button>().onClick.AddListener(() => AttachCallback(buttonName));
    }

    /// <summary>
    /// ボタン名に基づき、対応するアイテムの詳細を表示する
    /// </summary>
    private void AttachCallback(string btn)
    {
        switch (btn)
        {
            case "Item0":
                ItemManager.Instance.ShowItemDetail(0);
                break;
            case "Item1":
                ItemManager.Instance.ShowItemDetail(1);
                break;
            case "Item2":
                ItemManager.Instance.ShowItemDetail(2);
                break;
            case "Item3":
                ItemManager.Instance.ShowItemDetail(3);
                break;
            case "Item4":
                ItemManager.Instance.ShowItemDetail(4);
                break;
            case "Item5":
                ItemManager.Instance.ShowItemDetail(5);
                break;
        }
    }
}
