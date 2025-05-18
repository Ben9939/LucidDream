using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// フリールーム状態に入る際のダイアログイベント情報を保持する構造体
/// </summary>
[Serializable]
public struct EnterFreeRoamStateDialogEvent
{
    /// <summary>
    /// 前の状態がこの状態であった場合にトリガーするイベント
    /// </summary>
    public GameState triggerAfterState;

    /// <summary>
    /// 必要なシナリオ状態
    /// </summary>
    public ScenarioStateSO requiredScenarioState;

    /// <summary>
    /// 条件が真の場合にトリガーするイベント名
    /// </summary>
    public GameEventName trueEvent;

    /// <summary>
    /// 条件が偽の場合にトリガーするイベント名
    /// </summary>
    public GameEventName falseEvent;
}

/// <summary>
/// フリールーム状態を管理するクラス
/// プレイヤーの探索状態と、必要に応じたイベントのトリガーを処理します。
/// </summary>
public class FreeRoamState : GameStateBase
{
    // サブステート管理クラスのインスタンス（プライベートフィールド）
    private FreeRoam_SubStateManager subStateManager;

    /// <summary>
    /// サブステート管理インスタンス（読み取り専用）
    /// </summary>
    public FreeRoam_SubStateManager SubStateManager { get; private set; }

    // ダイアログイベントリスト
    private List<EnterFreeRoamStateDialogEvent> enterFreeRoamStateDialogEventList;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">現在の状態</param>
    /// <param name="enterFreeRoamStateDialogEventList">ダイアログイベントリスト</param>
    public FreeRoamState(GameStateManager gameStateManager, string sceneName, GameState gameState, List<EnterFreeRoamStateDialogEvent> enterFreeRoamStateDialogEventList)
        : base(gameStateManager, sceneName, gameState)
    {
        subStateManager = new FreeRoam_SubStateManager(this);
        SubStateManager = subStateManager;
        this.enterFreeRoamStateDialogEventList = enterFreeRoamStateDialogEventList;
    }

    /// <summary>
    /// 状態遷移時の初期処理
    /// 条件に応じたイベントのトリガーやサブステートへの移行を実施します。
    /// </summary>
    public override void EnterState()
    {
        Debug.Log("EnterFreeRoamState");
        var previousState = GameStateManager.Instance.PreviousGameState;

        // ゲームリセットが必要な場合の処理
        if (GameStateManager.HaveToRestGame)
        {
            GameStateManager.HaveToRestGame = false;
            GameEventManager.TriggerEvent(GameEventName.ResetGame);
        }

        // リストから条件に合致するダイアログイベントを検索する
        var dialogEvent = enterFreeRoamStateDialogEventList
                          .FirstOrDefault(e => e.triggerAfterState == previousState);

        // ダイアログイベントがデフォルト値でない場合
        if (!dialogEvent.Equals(default(EnterFreeRoamStateDialogEvent)))
        {
            var eventToTrigger = dialogEvent.requiredScenarioState.Condition
                                 ? dialogEvent.trueEvent
                                 : dialogEvent.falseEvent;

            // イベントが None でない場合はトリガーしてプレイヤー位置を初期化
            if (eventToTrigger != GameEventName.None)
            {
                GameEventManager.TriggerEvent(eventToTrigger);
                PlayerController.Instance.InitializePlayerPosition();
                return;
            }
        }

        // 条件に一致するイベントがない場合は、探索サブステートに移行する
        SubStateManager.EnterSubState(FreeRoamSubState.Exploration);
        PlayerController.Instance.InitializePlayerPosition();
    }

    /// <summary>
    /// 毎フレームの更新処理（サブステートの更新を実施）
    /// </summary>
    public override void UpdateState()
    {
        subStateManager.UpdateSubState();
    }

    /// <summary>
    /// 状態終了時の後処理（サブステートの終了処理を実施）
    /// </summary>
    public override void ExitState()
    {
        SubStateManager.ExitSubState();
    }
}