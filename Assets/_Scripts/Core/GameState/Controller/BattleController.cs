using UnityEngine;

/// <summary>
/// バトルロジックの管理を行うコントローラクラス。
/// ControllerBaseを継承し、ゲーム状態のイベントに連動してバトルロジックの初期化、更新、クリーンアップ処理を実施する。
/// </summary>
public class BattleController : ControllerBase
{
    [Header("Battle Logic Manager")]
    [SerializeField]
    private BattleLogicManager battleLogic;

    /// <summary>
    /// ゲーム状態開始時の処理
    /// バトルロジックの初期化を実行する。
    /// </summary>
    protected override void Initialize()
    {
        // バトルロジックの初期化処理
        battleLogic.Initialize();
        AudioManager.Instance.PlayBGM(BGMSoundData.BGM.Battle);
    }

    /// <summary>
    /// ゲーム状態更新時の処理
    /// バトルロジックの更新処理を実行する。
    /// </summary>
    protected override void OnUpdate()
    {
        // バトルロジックの更新処理
        battleLogic.UpdateBattle();
    }

    /// <summary>
    /// ゲーム状態終了時の処理
    /// Pythonスクリプトの停止とバトルロジックのクリーンアップを実行する。
    /// </summary>
    protected override void Cleanup()
    {
        // Pythonプロセスマネージャーを使用してPythonスクリプトを停止する
        PythonProcessManager.Instance.StopPythonScript();
        // バトルロジックのクリーンアップ処理
        battleLogic.Cleanup();
        AudioManager.Instance.StopBGM();
    }
}
