using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// FreeRoam_DialogController は、自由移動状態の対話サブステートを管理します。
/// 対話 UI の初期化、対話ロジックの実行、及び状態遷移を制御します。
/// </summary>
public class FreeRoam_DialogController : FreeRoam_ControllerBase
{
    public static FreeRoam_DialogController Instance { get; private set; }

    [Header("UI Prefabs")]
    [SerializeField] private GameObject dialogUIContainer;  // 対話 UI コンテナ
    [SerializeField] private GameObject npcUIPrefab;          // NPC 対話 UI プレハブ
    [SerializeField] private GameObject objectUIPrefab;       // オブジェクト対話 UI プレハブ
    [SerializeField] private GameObject eventUIPrefab;        // イベント対話 UI プレハブ

    // 現在実行中の対話ロジック
    private DialogProcessor currentDialogLogic;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// ダイアログを初期化する。
    /// 指定した UI プレハブを元にインスタンスを生成し、対話ロジックを設定します。
    /// </summary>
    /// <param name="uiPrefab">使用する UI プレハブ</param>
    /// <param name="dialogGraph">対話グラフ</param>
    private void InitializeDialog(GameObject uiPrefab, DialogGraph dialogGraph)
    {
        GameObject uiInstance = Instantiate(uiPrefab, dialogUIContainer.transform);
        IDialogUI dialogUI = uiInstance.GetComponent<IDialogUI>();

        currentDialogLogic = new DialogProcessor(dialogUI);
        currentDialogLogic.SetDialogGraph(dialogGraph);
        currentDialogLogic.onDialogEnd += () =>
        {
            Destroy(uiInstance);
            EndDialog();
        };

        // 対話モードに切り替える
        FreeRoamState freeRoamState = GameStateManager.Instance.GetCurrentState() as FreeRoamState;
        if (freeRoamState != null)
        {
            freeRoamState.SubStateManager.SwitchSubState(FreeRoamSubState.Dialog);
        }
        currentDialogLogic.StartDialog();
    }

    /// <summary>
    /// イベント対話を開始する。
    /// </summary>
    /// <param name="dialogGraph">対話グラフ</param>
    /// <param name="eventDialogUIType">イベント対話 UI タイプ</param>
    public void StartEventDialog(DialogGraph dialogGraph, EventDialogUIType eventDialogUIType)
    {
        GameObject uiPrefab = null;

        switch (eventDialogUIType)
        {
            case EventDialogUIType.NpcUI:
                uiPrefab = npcUIPrefab;
                break;
            case EventDialogUIType.ObjectUI:
                uiPrefab = objectUIPrefab;
                break;
            case EventDialogUIType.EventUI:
                uiPrefab = eventUIPrefab;
                break;
            default:
                uiPrefab = npcUIPrefab;
                break;
        }

        InitializeDialog(uiPrefab, dialogGraph);
    }

    /// <summary>
    /// NPC 対話を開始する。
    /// </summary>
    /// <param name="npcData">NPC データ</param>
    public void StartNPCDialog(NpcDataSO npcData)
    {
        DialogGraph dialogGraph = DialogStateManager.Instance.GetCurrentDialog(npcData);
        if (dialogGraph == null)
        {
            Debug.LogWarning("No dialog available for NPC.");
            return;
        }
        InitializeDialog(npcUIPrefab, dialogGraph);
    }

    /// <summary>
    /// オブジェクト対話を開始する。
    /// </summary>
    /// <param name="objectData">オブジェクトデータ</param>
    public void StartObjectDialog(ObjectDataSO objectData)
    {
        DialogGraph dialogGraph = DialogStateManager.Instance.GetCurrentDialog(objectData);
        if (dialogGraph == null)
        {
            Debug.LogWarning("No dialog available for object.");
            return;
        }
        InitializeDialog(objectUIPrefab, dialogGraph);
    }

    /// <summary>
    /// 対話終了後の処理。探索状態に戻ります。
    /// </summary>
    private void EndDialog()
    {
        currentSubState.RequestSubStateChange(FreeRoamSubState.Exploration);
        currentDialogLogic = null;
    }

    /// <summary>
    /// 管理対象のサブステートは Dialog です。
    /// </summary>
    protected override FreeRoamSubState ControlledSubState => FreeRoamSubState.Dialog;

    /// <summary>
    /// 対話状態中の更新処理。対話入力の更新を行います。
    /// </summary>
    protected override void OnSubStateUpdate()
    {
        currentDialogLogic.HandleSelectionInput();
        currentDialogLogic.HandleConfirmInput();
    }

    protected override void OnSubStateEnter() { }

    /// <summary>
    /// 対話状態から出るときの処理。対話を終了します。
    /// </summary>
    protected override void OnSubStateExit()
    {
        EndDialog();
    }
}
