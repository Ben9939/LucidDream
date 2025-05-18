using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// Scenario_SceneChangeNode は、シーン変更を実行するノードです。
/// TargetState を保持し、次のシーンへの遷移情報を提供します。
/// </summary>
public class Scenario_SceneChangeNode : Scenario_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;
    [SerializeField] public GameState TargetState;
}
