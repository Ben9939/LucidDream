using UnityEngine;
using static SceneMapManager;

/// <summary>
/// フリーロームシーンへの切り替えを管理するダイアログイベントのScriptableObjectです。
/// シーン遷移イベントをハンドルします。
/// </summary>
[CreateAssetMenu(menuName = "GameEvent/DialogEvent/FreeRoamSceneSwitch")]
public class DialogFreeRoamSceneSwitchEventSO : ScriptableObject
{
    [SerializeField] private SceneBoundaryName targetSceneBound;

    /// <summary>
    /// シーンに入る際の処理を実行します。
    /// 指定されたシーン境界に基づいてシーン切り替えイベントを発火します。
    /// </summary>
    public void HandleEnterScene()
    {
        SceneMapManager.Instance.SceneSwitchEvent(targetSceneBound);
        if (targetSceneBound == SceneBoundaryName.Level7_1)
        {
            SceneMapManager.Instance.SetBoundaryOneWay(SceneBoundaryName.Level7_1, true);
        }
        
    }

    /// <summary>
    /// シーンを設定する処理を実行します。
    /// プレイヤーの現在のシーン境界を更新します。
    /// </summary>
    public void HandleSetScene()
    {
        PlayerController.Instance.playerData.CurrentBoundary = targetSceneBound;
    }
}
