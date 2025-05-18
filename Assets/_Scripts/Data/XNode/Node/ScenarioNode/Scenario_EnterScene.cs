using UnityEngine;
using XNode;

/// <summary>
/// Scenario_EnterScene は、シーンへの入場を示すノードです。
/// 指定された GameState に遷移するための情報を保持します。
/// </summary>
public class Scenario_EnterScene : Scenario_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;
    [SerializeField] public GameState TargetState;
}

