using UnityEngine;

/// <summary>
/// QRコード読み取り状態を管理するクラス
/// QRコード読み取りシーンでの状態遷移や更新処理を担当します。
/// </summary>
public class ReadQRCodeState : GameStateBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameStateManager">ゲーム状態管理インスタンス</param>
    /// <param name="sceneName">シーン名</param>
    /// <param name="gameState">現在の状態</param>
    public ReadQRCodeState(GameStateManager gameStateManager, string sceneName, GameState gameState)
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

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ReadQRCodeState : GameStateBase
//{
//    public ReadQRCodeState(GameStateManager gameStateManager, string sceneName, GameState gameState)
//        : base(gameStateManager, sceneName, gameState) { }

//    public override void EnterState()
//    {
//        base.EnterState();
//    }

//    public override void UpdateState()
//    {
//        base.UpdateState();
//    }

//    public override void ExitState()
//    {
//        base.ExitState();
//    }
//}