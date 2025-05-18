/// <summary>
/// メインタイトル状態を管理するクラス
/// メインタイトルシーンでの処理を実施します。
/// </summary>
public class MainTitleState : GameStateBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">現在の状態</param>
    public MainTitleState(GameStateManager gameStateManager, string sceneName, GameState gameState)
        : base(gameStateManager, sceneName, gameState)
    {
    }

    /// <summary>
    /// 状態開始時の処理
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
    /// 状態終了時の処理
    /// </summary>
    public override void ExitState()
    {
        base.ExitState();
    }
}
