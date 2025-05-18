using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// Scenario_Option は、分岐ノードにおける各選択肢の条件情報を保持します。
/// 必要な状態、比較結果、及び優先度を設定します。
/// </summary>
[System.Serializable]

public class Scenario_Option
{
    [Header("Option Condition")]
    public ScenarioStateSO requiredUnitState; 

    [Header("Comparison Result")]
    public ComparisonResult requiredComparison;

    [Header("Priority (Higher number means higher priority)")]
    public int priority;
}


/// <summary>
/// Scenario_BranchNode は、条件に基づく分岐処理を行うノードです。
/// 各選択肢（Scenario_Option）の条件を順に評価し、最も優先度の高い選択肢に接続されたノードを返します。
/// </summary>
[NodeWidth(300)]
public class Scenario_BranchNode : Scenario_BaseNode
{
    [Input] public int entry;

    [Output(dynamicPortList = true)]
    public List<Scenario_Option> options;

    /// <summary>
    /// すべての BranchOption を評価し、条件を満たす中で最も優先度の高い分岐先ノードを返します。
    /// </summary>
    /// <returns>次の Scenario_BaseNode への参照（条件を満たさなければ null）</returns>
    public Scenario_BaseNode GetNextBranchNode()
    {
        // 現在の最良の優先度とそのインデックスを記録する
        int bestPriority = int.MinValue;
        int bestIndex = -1;

        for (int i = 0; i < options.Count; i++)
        {
            Scenario_Option branchOption = options[i];

            // 1) 条件のチェック
            bool conditionMet = false;
            if (branchOption.requiredUnitState == null)
            {
                // 条件指定がなければ自動通過
                conditionMet = true;
            }
            else
            {
                // requiredUnitState.Condition で条件を評価（必要に応じて Compare() メソッドも利用可能）
                if (branchOption.requiredUnitState.Condition)
                {
                    conditionMet = true;
                }
            }

            // 2) 条件が満たされている場合、優先度を比較する
            if (conditionMet)
            {
                if (branchOption.priority > bestPriority)
                {
                    bestPriority = branchOption.priority;
                    bestIndex = i;
                }
            }
        }

        // 3) 条件を満たす最良の選択肢が見つかった場合、対応する出力ポートのノードを返す
        if (bestIndex != -1)
        {
            NodePort port = GetOutputPort("options " + bestIndex);
            if (port != null && port.IsConnected)
            {
                return port.Connection.node as Scenario_BaseNode;
            }
        }

        // 4) 条件を満たす選択肢がなければ null を返す
        return null;
    }
}
