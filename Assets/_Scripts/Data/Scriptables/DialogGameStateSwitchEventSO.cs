using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームステートの切り替えを管理するダイアログイベントのScriptableObjectです。
/// 選択に応じたゲーム状態への切り替えを実行します。
/// </summary>
[CreateAssetMenu(menuName = "GameEvent/DialogEvent/GameStateSwitch")]
public class DialogGameStateSwitchEventSO : ScriptableObject
{
    public GameState gameState;

    /// <summary>
    /// 選択が行われた際に、指定されたゲーム状態に切り替えます。
    /// </summary>
    public void HandleSelection()
    {
        GameStateManager.Instance.SwitchState(gameState);
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//[CreateAssetMenu(menuName = "GameEvent/DialogEvent/GameStateSwitch")]
//public class DialogGameStateSwitchEventSO : ScriptableObject
//{
//    public GameState gameState;

//    public void HandleSelection()
//    {
//        GameStateManager.Instance.SwitchState(gameState);   
//    }
//}
