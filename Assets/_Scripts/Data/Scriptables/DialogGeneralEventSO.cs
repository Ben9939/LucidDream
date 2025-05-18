using UnityEngine;
using static PlayerController;
using static SceneMapManager;

/// <summary>
/// 一般的なダイアログイベントを管理するScriptableObjectです。
/// シナリオの進行やゲームリセットの処理を実行します。
/// </summary>
[CreateAssetMenu(menuName = "GameEvent/DialogEvent/GeneralEvent")]
public class DialogGeneralEventSO : ScriptableObject
{
    /// <summary>
    /// シナリオを進行させる処理を実行します。
    /// </summary>
    public void HandleAdvanceScenario()
    {
        ScenarioManager.Instance.AdvanceScenario();
    }

    /// <summary>
    /// ゲームをリセットする処理を実行します。
    /// </summary>
    public void ResetGame()
    {
        GameStateManager.HaveToRestGame = true;
    }
    public void SetBoundaryOneWay()
    {
        SceneMapManager.Instance.SetBoundaryOneWay(SceneMapManager.SceneBoundaryName.Level4_4, true);

    }
    //public void SetBoundaryOneWayEvent(SceneMapManager.SceneBoundaryName boundaryName, bool state)
    //{
    //    SceneMapManager.Instance.SetBoundaryOneWay(boundaryName, state);

    //}
}

//using UnityEngine;
//using static PlayerController;
//using static SceneMapManager;

//[CreateAssetMenu(menuName = "GameEvent/DialogEvent/GeneralEvent")]
//public class DialogGeneralEventSO : ScriptableObject
//{
//    public void HandleAdvanceScenario()
//    {
//        ScenarioManager.Instance.AdvanceScenario();
//    }
//    public void ResetGame()
//    {
//        GameStateManager.HaveToRestGame = true;
//    }

//}
