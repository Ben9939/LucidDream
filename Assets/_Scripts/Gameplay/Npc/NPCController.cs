using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// NPC の対話および移動を管理するクラス
/// </summary>
public class NPCController : MonoBehaviour, Interactable
{
    [Header("NPC Settings")]
    [SerializeField] private NpcMovementTypeSO npcMovementType;
    [SerializeField] private NpcDataSO npcData;

    // プライベートな静的変数は camelCase
    private static List<NPCController> activeNPCs = new List<NPCController>();

    private FreeRoamState freeRoamState;

    [Header("Dialog Hint Settings")]
    [SerializeField] private GameObject dialogHintPrefab;
    private GameObject dialogHintInstance;
    private bool showDialogHint = true;
    private int onShowDialogHintDistance = 3;

    [Header("Reset Settings")]
    public UnityAction resetNpcAction;

    /// <summary>
    /// Awake：NPC の登録、対話ヒントの生成、リセットイベントの購読を行う
    /// </summary>
    private void Awake()
    {
        activeNPCs.Add(this);
        if (dialogHintPrefab == null)
        {
            dialogHintPrefab = Resources.Load<GameObject>("Prefabs/DialogHint"); // Path: Resources/Prefabs/DialogHint
        }

        // 対話ヒントインスタンスを生成（NPC 上部に配置）
        if (dialogHintPrefab != null)
        {
            dialogHintInstance = Instantiate(dialogHintPrefab, transform);
            dialogHintInstance.transform.localPosition = new Vector3(0, 1f, 0);
            dialogHintInstance.SetActive(showDialogHint);
        }
        resetNpcAction += ResetNpc;
        GameEventManager.StartListening(GameEventName.ResetGame, resetNpcAction);
    }

    private void OnDestroy()
    {
        activeNPCs.Remove(this);
    }

    private void OnDisable()
    {
        activeNPCs.Remove(this);
    }

    /// <summary>
    /// 全 NPC の更新処理を行う
    /// </summary>
    public static void UpdateAllNPCs()
    {
        foreach (var npc in activeNPCs)
        {
            npc.HandleNPCUpdate();
        }
    }

    /// <summary>
    /// NPC の移動と対話ヒントの更新を行う
    /// </summary>
    private void HandleNPCUpdate()
    {
        if (npcMovementType != null)
        {
            StartCoroutine(npcMovementType.Move(this));
        }
        UpdateDialogHint();
    }

    /// <summary>
    /// NPC と対話を開始する
    /// </summary>
    public void Interact()
    {
        freeRoamState = GameStateManager.Instance.GetCurrentState() as FreeRoamState;
        if (freeRoamState == null)
        {
            Debug.LogWarning("No FreeRoam state currently.");
            return;
        }
        FreeRoam_DialogController.Instance.StartNPCDialog(npcData);
    }

    /// <summary>
    /// 対話ヒントの表示を更新する
    /// </summary>
    public void UpdateDialogHint()
    {
        Vector2 npcPos = transform.position;
        Vector2 playerPos = PlayerController.Instance.GetPlayerPosition();
        if (dialogHintInstance != null &&
            Vector2.Distance(npcPos, playerPos) < onShowDialogHintDistance)
        {
            showDialogHint = true;
            dialogHintInstance.SetActive(true);
        }
        else
        {
            showDialogHint = false;
            dialogHintInstance.SetActive(false);
        }
    }

    /// <summary>
    /// NPC データをリセットする
    /// </summary>
    public void ResetNpc()
    {
        npcData?.ResetData();
    }

    public void OnApplicationQuit()
    {
        ResetNpc();
        GameEventManager.StopListening(GameEventName.ResetGame, resetNpcAction);
        resetNpcAction -= ResetNpc;
    }
}
