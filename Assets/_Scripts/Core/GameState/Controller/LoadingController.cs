using System;
using UnityEngine;

/// <summary>
/// LoadingControllerは、ゲーム状態に応じた適切なローダー（CameraLoaderまたはQrCodeLoader）を自動選択し、
/// ロード処理を開始することで、ゲーム状態の遷移を管理するクラスです。
/// 外部のGameStateManagerから目標状態（TargetState）が指定された場合、その状態に応じたローダーを選び、
/// ロード完了時に状態遷移を実行します。
/// </summary>
public class LoadingController : ControllerBase
{
    [Header("Loaders")]
    [SerializeField] private CameraLoader cameraLoader;
    [SerializeField] private QrCodeLoader qrCodeLoader;

    // 外部より GameStateManager により指定される目標状態
    public GameState TargetState { get; set; }

    // 現在利用中のローダー
    private IScriptLoader currentLoader;

    /// <summary>
    /// ゲーム状態開始時の処理。
    /// 対象のローダーを自動選択し、ロード処理を開始する。
    /// </summary>
    protected override void Initialize()
    {
        AudioManager.Instance.StopBGM();
        // LoadingState が存在する場合、TargetState を取得する
        if (GameStateManager.Instance.LoadingState != null)
        {
            TargetState = GameStateManager.Instance.LoadingState.TargetState;
        }

        // TargetState に応じたローダーを自動選択する
        if (TargetState == GameState.Battle || TargetState == GameState.Puzzle)
        {
            currentLoader = cameraLoader;
        }
        else if (TargetState == GameState.ReadQrCode)
        {
            currentLoader = qrCodeLoader;
        }
        else
        {
            Debug.LogError($"No loader configured for state: {TargetState}");
            return;
        }

        // 選択したローダーが存在する場合、ロード完了イベントを登録してロードを開始する
        if (currentLoader != null)
        {
            currentLoader.OnLoaded += OnLoaderCompleted;
            currentLoader.StartLoading();
        }
    }

    /// <summary>
    /// ゲーム状態更新時の処理。
    /// ローダーのアップデート処理を実行する。
    /// </summary>
    protected override void OnUpdate()
    {
        if (currentLoader != null)
        {
            currentLoader.UpdateHandle();
        }
    }

    /// <summary>
    /// ゲーム状態終了時の処理。
    /// 必要に応じてクリーンアップ処理を実行する（ここでは未使用）。
    /// </summary>
    protected override void Cleanup()
    {
        // 必要に応じてロード処理を停止する場合は以下を有効化
        // _currentLoader?.StopLoading();
    }

    /// <summary>
    /// ロード完了時のコールバック。
    /// ロード完了後、イベントの解除と状態遷移を実行する。
    /// </summary>
    private void OnLoaderCompleted()
    {
        Debug.Log("LoadingController: Loader completed, switching state.");
        if (currentLoader != null)
        {
            currentLoader.OnLoaded -= OnLoaderCompleted;
        }
        GameStateManager.Instance.SwitchState(TargetState);
    }
}
