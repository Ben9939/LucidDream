using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// Scenario_ConditionNode は、シナリオ分岐条件を評価するノードです。
/// ScenarioState を保持し、条件が満たされたかどうかをチェックします。
/// </summary>
public class Scenario_ConditionNode : Scenario_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;
    [SerializeField] public ScenarioStateSO ScenarioState;
}
