using System;
using UnityEngine;

/// <summary>
/// すべてのゲーム状態クラスの基底クラス
/// 状態の遷移、更新、終了時の共通処理を提供します。
/// </summary>
public abstract class GameStateBase
{
    /// <summary>
    /// ゲーム状態管理クラスのインスタンス
    /// </summary>
    protected GameStateManager gameStateManager;

    // 状態がアクティブかどうかのフラグ（初期値は false）
    private bool isActive = false;

    // 現在のゲーム状態
    private GameState gameState;

    /// <summary>
    /// 現在のゲーム状態を取得します。
    /// </summary>
    public GameState GameState { get { return gameState; } }

    /// <summary>
    /// 状態に対応するシーン名
    /// </summary>
    public string SceneName { get; private set; }

    /// <summary>
    /// 状態完了時に発火するイベント（状態遷移の通知用）
    /// </summary>
    public event Action<GameState> OnStateFinished;

    /// <summary>
    /// 状態開始時に発火するイベント
    /// </summary>
    public event Action OnStateEnter;

    /// <summary>
    /// 状態更新時に発火するイベント
    /// </summary>
    public event Action OnStateUpdate;

    /// <summary>
    /// 状態終了時に発火するイベント
    /// </summary>
    public event Action OnStateExit;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">初期状態</param>
    public GameStateBase(GameStateManager gameStateManager, string sceneName = null, GameState gameState = GameState.None)
    {
        this.gameStateManager = gameStateManager;
        SceneName = sceneName;
        this.gameState = gameState;
    }

    /// <summary>
    /// 状態開始時の処理を実施し、アクティブ状態に設定します。
    /// </summary>
    public virtual void EnterState()
    {
        isActive = true;
        OnStateEnter?.Invoke();
    }

    /// <summary>
    /// 毎フレーム呼び出される更新処理（状態がアクティブの場合のみ更新）
    /// </summary>
    public virtual void UpdateState()
    {
        if (!isActive)
            return; // 状態が非アクティブの場合は更新しない

        OnStateUpdate?.Invoke();
    }

    /// <summary>
    /// 状態終了時の処理を実施し、非アクティブ状態に設定します。
    /// </summary>
    public virtual void ExitState()
    {
        isActive = false;
        OnStateExit?.Invoke();
    }

    /// <summary>
    /// 状態変更を要求するためのメソッド
    /// </summary>
    /// <param name="nextState">次の状態</param>
    public void RequestStateChange(GameState nextState)
    {
        OnStateFinished?.Invoke(nextState);
    }
}
