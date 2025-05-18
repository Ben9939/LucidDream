/// <summary>
/// クレジットスクロール状態を管理するクラス
/// ゲーム内でクレジットスクロール時の状態遷移を処理します。
/// </summary>
public class CreditScrolleState : GameStateBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">現在の状態</param>
    public CreditScrolleState(GameStateManager gameStateManager, string sceneName, GameState gameState)
        : base(gameStateManager, sceneName, gameState)
    {
    }

    /// <summary>
    /// 状態遷移時の初期処理
    /// </summary>
    public override void EnterState()
    {
        base.EnterState();
    }

    /// <summary>
    /// 毎フレームの更新処理
    /// </summary>
    public override void UpdateState()
    {
        base.UpdateState();
    }

    /// <summary>
    /// 状態終了時の後処理
    /// </summary>
    public override void ExitState()
    {
        base.ExitState();
    }
}
