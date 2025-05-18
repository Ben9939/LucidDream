using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラの追従処理を行うクラスです。
/// 対象の Transform を追従し、スムーズな移動やターゲットの切り替えを提供します。
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform currentTarget;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private bool useSmoothFollow = true;
    [SerializeField] private Transform[] targets;
    [SerializeField] private Transform defaultTarget; // デフォルトのターゲット

    private int currentTargetIndex = 0;

    private void LateUpdate()
    {
        // 現在のターゲットが null の場合、デフォルトターゲットを使用する
        if (currentTarget == null)
        {
            if (defaultTarget != null)
            {
                currentTarget = defaultTarget;
                // Debug.LogWarning("現在のターゲットが null のため、デフォルトターゲットに切り替えました。");
            }
            else
            {
                // Debug.LogWarning("現在のターゲットとデフォルトターゲットが両方とも null です。");
                return;
            }
        }

        FollowTarget();
    }

    /// <summary>
    /// 対象の Transform を追従してカメラを移動させます。
    /// </summary>
    private void FollowTarget()
    {
        Vector3 targetPosition = currentTarget.position + offset;

        if (useSmoothFollow)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        transform.LookAt(currentTarget);
    }

    /// <summary>
    /// 指定されたターゲットにカメラを切り替えます。
    /// </summary>
    /// <param name="newTarget">新しいターゲットの Transform</param>
    public void SwitchTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError("指定されたターゲットが null のため、切り替えできません！");
            return;
        }

        currentTarget = newTarget;
        Debug.Log($"ターゲットを切り替えました: {newTarget.name}");
    }

    /// <summary>
    /// ターゲット配列内で次のターゲットにカメラを切り替えます。
    /// </summary>
    public void SwitchToNextTarget()
    {
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("利用可能なターゲットがありません！");
            return;
        }

        currentTargetIndex = (currentTargetIndex + 1) % targets.Length;
        currentTarget = targets[currentTargetIndex];
        Debug.Log($"次のターゲットに切り替えました: {currentTarget.name}");
    }

    /// <summary>
    /// カメラのオフセット値を更新します。
    /// </summary>
    /// <param name="newOffset">新しいオフセット値</param>
    public void UpdateOffset(Vector3 newOffset)
    {
        offset = newOffset;
        Debug.Log($"オフセット値を更新しました: {newOffset}");
    }

    /// <summary>
    /// ターゲット配列に新しいターゲットを追加します。
    /// </summary>
    /// <param name="newTarget">追加するターゲットの Transform</param>
    public void AddTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError("null のターゲットは追加できません！");
            return;
        }

        var targetList = new List<Transform>(targets ?? new Transform[0]);
        targetList.Add(newTarget);
        targets = targetList.ToArray();

        Debug.Log($"ターゲット {newTarget.name} を追加しました！");
    }
}

