/// <summary>
/// 戦闘状態クラス
/// ゲーム内の戦闘シーンにおける状態管理を行う。
/// </summary>
public class BattleState : GameStateBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理クラスのインスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">前回のゲーム状態</param>
    public BattleState(GameStateManager gameStateManager, string sceneName, GameState gameState)
        : base(gameStateManager, sceneName, gameState)
    {
    }

    /// <summary>
    /// 戦闘状態への遷移時に実行される処理
    /// </summary>
    public override void EnterState()
    {
        base.EnterState();
    }

    /// <summary>
    /// 戦闘状態の更新処理
    /// </summary>
    public override void UpdateState()
    {
        base.UpdateState();
    }

    /// <summary>
    /// 戦闘状態からの退出時に実行される処理
    /// </summary>
    public override void ExitState()
    {
        base.ExitState();
    }
}