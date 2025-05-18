using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NPCの自動移動を管理するScriptableObjectです。  
/// 一定間隔でNPCがランダムな方向へ移動し、指定範囲内に収まるよう制限します。
/// </summary>
[CreateAssetMenu(fileName = "NpcAutoMove", menuName = "Unit/NPC/Movement/AutoMove")]
public class NpcAutoMoveSO : NpcMovementTypeSO
{
    [Tooltip("移動速度")]
    public float moveSpeed = 2f;
    [Tooltip("各移動ステップ間の待機時間")]
    public float stepInterval = 0.5f;
    [Tooltip("移動可能な範囲（X, Y方向の最大距離）")]
    public Vector2 movementRange = new Vector2(5, 5);

    // プライベート変数はcamelCase（アンダースコアは使用しない）
    private bool isMoving = false;

    /// <summary>
    /// 移動可能な方向のリスト（8方向）を定義します。
    /// </summary>
    private static readonly List<Vector3> directions = new List<Vector3>
    {
        Vector3.up, Vector3.down, Vector3.left, Vector3.right,
        new Vector3(1, 1, 0).normalized, new Vector3(-1, 1, 0).normalized,
        new Vector3(1, -1, 0).normalized, new Vector3(-1, -1, 0).normalized
    };

    /// <summary>
    /// NPCを自動移動させるコルーチンです。  
    /// 各ステップごとに最適な方向を選択し、NPCをその方向に移動させます。
    /// </summary>
    public override IEnumerator Move(NPCController npc)
    {
        if (!isMoving)
        {
            isMoving = true;
            yield return new WaitForSeconds(stepInterval);

            Vector3 startPosition = npc.transform.position;
            Vector3 bestDirection = Vector3.zero;
            float shortestDistance = float.MaxValue;

            // 各方向を試して、移動範囲内かつランダムな値が最も小さい方向を選択
            foreach (Vector3 direction in directions)
            {
                Vector3 potentialPosition = startPosition + direction;
                float distance = Random.value; // ランダムな値で方向をランダム化

                if (IsWithinRange(potentialPosition, startPosition) && distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestDirection = direction;
                }
            }

            if (bestDirection != Vector3.zero)
            {
                Vector3 targetPos = startPosition + bestDirection;
                yield return MoveTo(npc, targetPos);
            }
            isMoving = false;
        }
    }

    /// <summary>
    /// 指定された位置が、開始位置から指定範囲内にあるかどうかを判定します。
    /// </summary>
    private bool IsWithinRange(Vector3 position, Vector3 startPosition)
    {
        return Mathf.Abs(position.x - startPosition.x) <= movementRange.x / 2 &&
               Mathf.Abs(position.y - startPosition.y) <= movementRange.y / 2;
    }

    /// <summary>
    /// NPCをターゲット位置へ滑らかに移動させるコルーチンです。
    /// 移動が完了するまで毎フレーム更新し、最終的に位置を確実に合わせます。
    /// </summary>
    private IEnumerator MoveTo(NPCController npc, Vector3 targetPos)
    {
        while ((targetPos - npc.transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            npc.transform.position = Vector3.MoveTowards(npc.transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        npc.transform.position = targetPos; // 最終位置を正確に設定
    }
}

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//[CreateAssetMenu(fileName = "NpcAutoMove", menuName = "Unit/NPC/Movement/AutoMove")]
//public class NpcAutoMoveSO : NpcMovementTypeSO
//{
//    public float moveSpeed = 2f;
//    public float stepInterval = 0.5f;
//    public Vector2 movementRange = new Vector2(5, 5);
//    private bool _isMoving = false;

//    private static readonly List<Vector3> directions = new List<Vector3>
//    {
//        Vector3.up, Vector3.down, Vector3.left, Vector3.right,
//        new Vector3(1,1,0).normalized, new Vector3(-1,1,0).normalized,
//        new Vector3(1,-1,0).normalized, new Vector3(-1,-1,0).normalized
//    };

//    public override IEnumerator Move(NPCController npc)
//    {
//        if (!_isMoving)
//        {
//            _isMoving = true;
//            yield return new WaitForSeconds(stepInterval);

//            Vector3 startPosition = npc.transform.position;
//            Vector3 bestDirection = Vector3.zero;
//            float shortestDistance = float.MaxValue;

//            foreach (Vector3 direction in directions)
//            {
//                Vector3 potentialPosition = startPosition + direction;
//                float distance = Random.value;

//                if (IsWithinRange(potentialPosition, startPosition) && distance < shortestDistance)
//                {
//                    shortestDistance = distance;
//                    bestDirection = direction;
//                }
//            }

//            if (bestDirection != Vector3.zero)
//            {
//                Vector3 targetPos = startPosition + bestDirection;
//                yield return MoveTo(npc, targetPos);
//            }
//        _isMoving = false;
//        }

//    }

//    private bool IsWithinRange(Vector3 position, Vector3 startPosition)
//    {
//        return Mathf.Abs(position.x - startPosition.x) <= movementRange.x / 2 &&
//               Mathf.Abs(position.y - startPosition.y) <= movementRange.y / 2;
//    }

//    private IEnumerator MoveTo(NPCController npc, Vector3 targetPos)
//    {
//        while ((targetPos - npc.transform.position).sqrMagnitude > Mathf.Epsilon)
//        {
//            npc.transform.position = Vector3.MoveTowards(npc.transform.position, targetPos, moveSpeed * Time.deltaTime);
//            yield return null;
//        }

//        npc.transform.position = targetPos; // 確保最終對齊
//    }
//}
