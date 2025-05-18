using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// イベント状態における対話処理を管理するクラス。
/// ゲーム状態に応じた対話UIの生成や、入力ハンドリングを行う。
/// </summary>
public class EventStateController : ControllerBase
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject dialogUIContainer;   // 対話UIを配置するコンテナ
    [SerializeField] private GameObject eventUIPrefab;         // NPC対話UIのプレハブ

    // 現在実行中の対話ロジックを保持する変数
    private DialogProcessor dialogProcessor;

    /// <summary>
    /// ゲーム状態開始時の処理。
    /// 現在のダイアロググラフを元に対話イベントを開始する。
    /// </summary>
    protected override void Initialize()
    {
        StartEventDialog(ScenarioManager.Instance.CurrentDialogGraph);
    }

    /// <summary>
    /// ゲーム状態更新時の処理。
    /// 各フレームで対話処理の入力ハンドリングを行う。
    /// </summary>
    protected override void OnUpdate()
    {
        dialogProcessor.HandleSelectionInput();
        dialogProcessor.HandleConfirmInput();
    }

    /// <summary>
    /// ゲーム状態終了時の処理。
    /// 対話終了時のクリーンアップ処理（必要に応じて拡張可能）。
    /// </summary>
    protected override void Cleanup()
    {
    }

    /// <summary>
    /// 対話終了時の処理。
    /// ダイアログUIの破棄とシナリオの進行を実施する。
    /// </summary>
    private void EndDialog()
    {
        // GameStateManager.Instance.SwitchState(GameState.FreeRoam);
        ScenarioManager.Instance.AdvanceScenario();
    }

    /// <summary>
    /// イベント対話を開始する。
    /// 指定されたダイアロググラフに基づいて対話UIを生成し、対話ロジックを初期化する。
    /// </summary>
    /// <param name="dialogGraph">対話グラフ</param>
    public void StartEventDialog(DialogGraph dialogGraph)
    {
        if (dialogGraph == null)
        {
            Debug.LogWarning("No dialog available for NPC");
            return;
        }

        // 対話UIプレハブのインスタンスを生成し、コンテナの子として配置する
        GameObject uiInstance = Instantiate(eventUIPrefab, dialogUIContainer.transform);
        IDialogUI dialogUI = uiInstance.GetComponent<IDialogUI>();

        dialogProcessor = new DialogProcessor(dialogUI);
        dialogProcessor.SetDialogGraph(dialogGraph);
        dialogProcessor.onDialogEnd += () =>
        {
            Destroy(uiInstance);
            EndDialog();
        };

        dialogProcessor.StartDialog();
    }
}
