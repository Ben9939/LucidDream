using UnityEngine;

/// <summary>
/// ローディング状態を管理するクラス
/// 次の状態に遷移するための準備処理を実施します。
/// </summary>
public class LoadingState : GameStateBase
{
    /// <summary>
    /// 次に遷移するゲーム状態
    /// </summary>
    public GameState TargetState;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="targetState">遷移先の状態</param>
    /// <param name="gameState">現在の状態</param>
    public LoadingState(GameStateManager gameStateManager, string sceneName, GameState targetState, GameState gameState)
        : base(gameStateManager, sceneName, gameState)
    {
        TargetState = targetState;
    }

    /// <summary>
    /// 状態開始時の処理
    /// </summary>
    public override void EnterState()
    {
        base.EnterState();
    }

    /// <summary>
    /// 状態終了時の後処理
    /// ローディング状態は Loader の停止処理を直接管理せず、LoadingController が処理します。
    /// </summary>
    public override void ExitState()
    {
        base.ExitState();
        // ローディング状態は Loader の停止処理を直接管理しない
    }
}
