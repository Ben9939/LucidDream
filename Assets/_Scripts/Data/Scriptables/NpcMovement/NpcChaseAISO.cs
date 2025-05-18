using System.Collections;
using UnityEngine;

/// <summary>
/// NpcChaseAISO は、NPC がプレイヤーを追跡するための AI を実装した ScriptableObject です。
/// プレイヤーの位置および移動を予測し、A* などのアルゴリズムと組み合わせた移動処理を行います。
/// </summary>
[CreateAssetMenu(fileName = "NpcChaseAI", menuName = "Unit/NPC/Movement/ChaseAI")]
public class NpcChaseAISO : NpcMovementTypeSO
{
    public float moveSpeed = 2f;
    public float refreshRate = 0.2f;  // Refresh interval for target position
    public float predictionTime = 1f; // Time to predict player's movement
    public float predictionThreshold = 0.5f; // Threshold for directional similarity
    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;
    private bool isMoving = false; // 追跡中かどうか

    /// <summary>
    /// NPC の移動処理を実行します。
    /// プレイヤーの現在位置および予測位置に基づき、最適な方向へ移動させます。
    /// </summary>
    /// <param name="npc">移動対象の NPCController</param>
    /// <returns>コルーチン</returns>
    public override IEnumerator Move(NPCController npc)
    {
        if (!isMoving)
        {
            isMoving = true;
            yield return new WaitForSeconds(refreshRate);

            Vector3 currentPlayerPosition = PlayerController.Instance.GetPlayerPosition();
            playerVelocity = (currentPlayerPosition - lastPlayerPosition) / refreshRate;

            // プレイヤーの予測位置を算出
            Vector3 predictedPosition = currentPlayerPosition + playerVelocity * predictionTime;
            Vector3 directionToPlayer = (currentPlayerPosition - npc.transform.position).normalized;
            Vector3 directionToPrediction = (predictedPosition - npc.transform.position).normalized;

            // 方向の類似度を計算
            float directionSimilarity = Vector3.Dot(directionToPlayer, directionToPrediction);

            Vector3 targetPosition = directionSimilarity >= predictionThreshold ? predictedPosition : currentPlayerPosition;

            yield return MoveStepTowards(npc, targetPosition);

            lastPlayerPosition = currentPlayerPosition;
            isMoving = false;
        }
    }

    /// <summary>
    /// NPC を targetPosition に向かって一定速度で移動させる処理を行います。
    /// </summary>
    /// <param name="npc">移動対象の NPCController</param>
    /// <param name="targetPosition">目標位置</param>
    /// <returns>コルーチン</returns>
    private IEnumerator MoveStepTowards(NPCController npc, Vector3 targetPosition)
    {
        Vector3 bestDirection = FindBestDirection(npc, targetPosition);
        Vector3 newTargetPosition = npc.transform.position + bestDirection;

        while ((newTargetPosition - npc.transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, newTargetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        npc.transform.position = newTargetPosition;
    }

    /// <summary>
    /// NPC から targetPosition へ向かう際、Helper.Directions に基づいて最適な方向を選択します。
    /// </summary>
    /// <param name="npc">移動対象の NPCController</param>
    /// <param name="targetPosition">目標位置</param>
    /// <returns>正規化された最適方向</returns>
    private Vector3 FindBestDirection(NPCController npc, Vector3 targetPosition)
    {
        Vector3 bestDirection = Vector3.zero;
        float shortestDistance = float.MaxValue;

        foreach (Vector3 direction in Helper.Directions)
        {
            Vector3 potentialPosition = npc.transform.position + direction;
            float distance = (targetPosition - potentialPosition).sqrMagnitude;
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestDirection = direction;
            }
        }

        return bestDirection.normalized;
    }
}

//using System.Collections;
//using UnityEngine;

//[CreateAssetMenu(fileName = "NpcChaseAI", menuName = "Unit/NPC/Movement/ChaseAI")]
//public class NpcChaseAISO : NpcMovementTypeSO
//{
//    public float moveSpeed = 2f;
//    public float refreshRate = 0.2f;  // 刷新目标位置的时间间隔
//    public float predictionTime = 1f; // 预测玩家移动的时间
//    public float predictionThreshold = 0.5f; // 玩家方向与预测方向的偏差阈值
//    private Vector3 lastPlayerPosition;
//    private Vector3 playerVelocity;
//    private bool _isMoving = false;

//    public override IEnumerator Move(NPCController npc)
//    {
//        if (!_isMoving)
//        {
//            _isMoving = true;
//            yield return new WaitForSeconds(refreshRate);

//            Vector3 currentPlayerPosition = PlayerController.Instance.GetPlayerPosition();
//            playerVelocity = (currentPlayerPosition - lastPlayerPosition) / refreshRate;

//            // 预测玩家未来的位置
//            Vector3 predictedPosition = currentPlayerPosition + playerVelocity * predictionTime;
//            Vector3 directionToPlayer = (currentPlayerPosition - npc.transform.position).normalized;
//            Vector3 directionToPrediction = (predictedPosition - npc.transform.position).normalized;

//            // 计算方向的相似度
//            float directionSimilarity = Vector3.Dot(directionToPlayer, directionToPrediction);

//            Vector3 targetPosition = directionSimilarity >= predictionThreshold ? predictedPosition : currentPlayerPosition;

//            yield return MoveStepTowards(npc, targetPosition);


//            lastPlayerPosition = currentPlayerPosition;
//            _isMoving = false;
//        }
//    }

//    private IEnumerator MoveStepTowards(NPCController npc, Vector3 targetPosition)
//    {
//        Vector3 bestDirection = FindBestDirection(npc, targetPosition);
//        Vector3 newTargetPosition = npc.transform.position + bestDirection;

//        while ((newTargetPosition - npc.transform.position).sqrMagnitude > Mathf.Epsilon)
//        {
//            npc.transform.position = Vector3.MoveTowards(npc.transform.position, newTargetPosition, moveSpeed * Time.deltaTime);
//            yield return null;
//        }

//        npc.transform.position = newTargetPosition;
//    }

//    private Vector3 FindBestDirection(NPCController npc, Vector3 targetPosition)
//    {
//        Vector3 bestDirection = Vector3.zero;
//        float shortestDistance = float.MaxValue;

//        foreach (Vector3 direction in Helper.Directions)
//        {
//            Vector3 potentialPosition = npc.transform.position + direction;
//            float distance = (targetPosition - potentialPosition).sqrMagnitude;

//            if (distance < shortestDistance)
//            {
//                shortestDistance = distance;
//                bestDirection = direction;
//            }
//        }

//        return bestDirection.normalized;
//    }
//}
