// DialogState_Option.cs
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// ダイアログの分岐オプションを表します。
/// 条件、比較結果、優先度などを設定できます。
/// </summary>
[System.Serializable]
public class DialogState_Option
{
    [Header("State Condition")]
    public UnitStateSO requiredUnitState;  // 必要な状態 (例: hasKey)

    [Header("Comparison Result (Equal, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual)")]
    public ComparisonResult requiredComparison;

    [Header("Priority (Higher value = Higher priority)")]
    public int priority;
}

/// <summary>
/// ダイアログの分岐ノードです。
/// 入力ポート "entry" と動的な出力ポートリスト "options" を持ち、
/// 各分岐オプションの条件に基づいて次のノードを選択します。
/// </summary>
[NodeWidth(300)]
public class DialogState_BranchNode : DialogState_BaseNode
{
    [Input] public int entry;

    [Output(dynamicPortList = true)]
    public List<DialogState_Option> options;

    public override DialogStateNodeType GetType() => DialogStateNodeType.Branch;

    /// <summary>
    /// 分岐オプションを評価し、条件を満たし、かつ最も優先度の高い分岐に接続されたノードを返します。
    /// 条件に一致する分岐が存在しない場合は null を返します。
    /// </summary>
    public DialogState_BaseNode GetNextBranchNode()
    {
        // 現在の最高優先度とそのインデックスを保持する変数
        int bestPriority = int.MinValue;
        int bestIndex = -1;

        for (int i = 0; i < options.Count; i++)
        {
            DialogState_Option branchOption = options[i];

            // 条件が満たされているかを確認
            bool conditionMet = false;
            if (branchOption.requiredUnitState == null)
            {
                // 条件が設定されていない場合は自動的に合格
                conditionMet = true;
            }
            else
            {
                // Compare メソッドを使用して条件を評価
                if (branchOption.requiredUnitState.Compare(branchOption.requiredComparison))
                {
                    conditionMet = true;
                }
            }

            // 条件を満たす場合、優先度を比較して最高の分岐を選ぶ
            if (conditionMet && branchOption.priority > bestPriority)
            {
                bestPriority = branchOption.priority;
                bestIndex = i;
            }
        }

        // 条件に一致する分岐が見つかった場合、対応する出力ポートから次のノードを取得
        if (bestIndex != -1)
        {
            NodePort port = GetOutputPort("options " + bestIndex);
            if (port != null && port.IsConnected)
            {
                 return port.Connection.node as DialogState_BaseNode;
            }
        }

        // 条件に一致する分岐がない場合は null を返す
        return null;
    }
}
