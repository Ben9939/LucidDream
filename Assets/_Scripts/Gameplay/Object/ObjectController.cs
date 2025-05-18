using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 物件との対話を管理するクラスです。
/// Interactable インターフェイスを実装し、オブジェクトとの相互作用を提供します。
/// </summary>
public class ObjectController : MonoBehaviour, Interactable
{
    [SerializeField] private ObjectDataSO objectData;

    // アクティブな ObjectController のリスト
    private static List<ObjectController> activeObjects = new List<ObjectController>();

    private FreeRoamState freeRoamState;
    private GameObject dialogHintInstance;
    private GameObject dialogHintPrefab;
    private bool showDialogHint = true;
    private int onShowDialogHintDistance = 3;

    /// <summary>
    /// GameEvent "ResetGame" 発火時にオブジェクトのリセットを行うアクション
    /// </summary>
    public UnityAction resetObjectAction;

    private void Awake()
    {
        activeObjects.Add(this);

        // Resources からダイアログヒントのプレハブを読み込む（未設定の場合）
        if (dialogHintPrefab == null)
        {
            dialogHintPrefab = Resources.Load<GameObject>("Prefabs/DialogHint");
        }
        // ダイアログヒントインスタンスの生成と初期化
        if (dialogHintPrefab != null)
        {
            dialogHintInstance = Instantiate(dialogHintPrefab, transform);
            dialogHintInstance.transform.localPosition = new Vector3(0, 1f, 0);
            dialogHintInstance.SetActive(showDialogHint);
        }
        resetObjectAction += ResetObject;
        GameEventManager.StartListening(GameEventName.ResetGame, resetObjectAction);
    }

    private void OnDestroy()
    {
        activeObjects.Remove(this);
    }

    /// <summary>
    /// すべてのアクティブな ObjectController の更新処理を実行します。
    /// </summary>
    public static void UpdateAllObjects()
    {
        foreach (var obj in activeObjects)
        {
            obj.HandleObjectUpdate();
        }
    }

    /// <summary>
    /// このオブジェクトの更新処理（ダイアログヒントの更新）を実行します。
    /// </summary>
    private void HandleObjectUpdate()
    {
        UpdateDialogHint();
    }

    /// <summary>
    /// 物件との対話を開始します。
    /// FreeRoamState が取得できない場合は警告を出します。
    /// </summary>
    public void Interact()
    {
        freeRoamState = GameStateManager.Instance.GetCurrentState() as FreeRoamState;
        if (freeRoamState == null)
        {
            Debug.LogWarning("No FreeRoam state currently.");
            return;
        }
        FreeRoam_DialogController.Instance.StartObjectDialog(objectData);
    }

    /// <summary>
    /// プレイヤーとの距離に応じて、ダイアログヒントの表示状態を更新します。
    /// </summary>
    public void UpdateDialogHint()
    {
        Vector2 npcPosition = transform.position;
        Vector2 playerPosition = PlayerController.Instance.GetPlayerPosition();
        if (dialogHintInstance != null && Vector2.Distance(npcPosition, playerPosition) < onShowDialogHintDistance)
        {
            showDialogHint = true;
            dialogHintInstance.SetActive(true);
        }
        else
        {
            showDialogHint = false;
            if (dialogHintInstance != null)
            {
                dialogHintInstance.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 物件データを初期状態にリセットします。
    /// </summary>
    public void ResetObject()
    {
        objectData?.ResetData();
    }

    /// <summary>
    /// アプリケーション終了時に、オブジェクトをリセットし、リスナーの解除を行います。
    /// </summary>
    public void OnApplicationQuit()
    {
        ResetObject();
        GameEventManager.StopListening(GameEventName.ResetGame, resetObjectAction);
        resetObjectAction -= ResetObject;
    }
}
