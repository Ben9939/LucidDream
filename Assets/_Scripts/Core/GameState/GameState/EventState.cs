/// <summary>
/// イベント状態を管理するクラス
/// ゲーム内で特定のイベント処理を実行します。
/// </summary>
public class EventState : GameStateBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">現在の状態</param>
    public EventState(GameStateManager gameStateManager, string sceneName, GameState gameState)
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
    /// ここで MainTitle 固有の更新ロジックを追加可能です。
    /// </summary>
    public override void UpdateState()
    {
        base.UpdateState();
        // ここで MainTitle 固有の更新ロジックを追加する
    }

    /// <summary>
    /// 状態終了時の後処理
    /// </summary>
    public override void ExitState()
    {
        base.ExitState();
    }
}
