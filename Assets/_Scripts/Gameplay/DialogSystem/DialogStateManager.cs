using UnityEngine;

/// <summary>
/// DialogStateManager は、UnitDataSO に基づいて現在の対話グラフを取得し、対話状態を更新します。
/// </summary>
public class DialogStateManager : MonoBehaviour
{
    public static DialogStateManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 指定された UnitDataSO から現在の DialogGraph を返します。
    /// </summary>
    /// <param name="unitData">対象の UnitDataSO</param>
    /// <returns>現在の DialogGraph</returns>
    public DialogGraph GetCurrentDialog(UnitDataSO unitData)
    {
        UpdateDialogState(unitData);
        return (DialogGraph)unitData.CurrentDialogGraph;
    }

    /// <summary>
    /// UnitDataSO の対話状態を更新し、必要に応じて次のノードへ遷移させます。
    /// </summary>
    /// <param name="unitData">対象の UnitDataSO</param>
    private void UpdateDialogState(UnitDataSO unitData)
    {
        DialogStateGraph stateGraph = (DialogStateGraph)unitData.CurrentDialogStateGraph;

        if (stateGraph.currentNode == null)
        {
            stateGraph.StartDialog();
        }
        DialogState_BaseNode candidate = stateGraph.GetNextNode();
        candidate = (candidate is DialogState_EndNode || candidate == null) ? stateGraph.currentNode : candidate;

        if (candidate is DialogState_ContentNode contentNode)
        {
            if (contentNode.requiredUnitState == null || contentNode.requiredUnitState.StateComparisonResult)
            {
                stateGraph.currentNode = contentNode;
                unitData.CurrentDialogGraph = contentNode.dialogGraph;
            }
        }

        if (candidate is DialogState_BranchNode branchNode)
        {
            stateGraph.currentNode = branchNode;
            UpdateDialogState(unitData);
        }
    }
}
