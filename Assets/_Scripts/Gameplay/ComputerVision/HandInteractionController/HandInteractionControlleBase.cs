using UnityEngine;

/// <summary>
/// HandInteractionControllerBase は、手の動作追跡とインタラクション処理の基底クラスです。
/// 派生クラスは、必要なリソースを設定し、更新処理およびクリーンアップ処理を実装してください。
/// </summary>
public abstract class HandInteractionControllerBase : MonoBehaviour
{
    /// <summary>
    /// 派生クラスで注入されるリソース
    /// </summary>
    protected GameObject handTracker;         // 手の追跡オブジェクト
    protected System.Collections.Generic.List<Sprite> handPointSprite; // 手の状態に応じたスプライト
    protected float detectionRadius;          // 検出半径

    protected bool lastHandState = false;     // 前回の手の状態

    /// <summary>
    /// Awake：初期位置を原点に設定します
    /// </summary>
    protected virtual void Awake()
    {
        transform.position = Vector3.zero;
    }

    /// <summary>
    /// 手の状態に応じてスプライトを更新します
    /// </summary>
    protected void UpdateHandMotion()
    {
        bool currentState = handTracker.GetComponent<HandTracking>().handState;
        GetComponent<SpriteRenderer>().sprite = currentState ? handPointSprite[0] : handPointSprite[1];
    }

    /// <summary>
    /// 手の位置を更新します
    /// </summary>
    protected void UpdateHandPosition()
    {
        transform.position = handTracker.GetComponent<HandTracking>().smoothedPosition;
    }

    /// <summary>
    /// 指定レイヤー内で、手の周囲のインタラクト可能なコライダーを取得します
    /// </summary>
    protected Collider2D[] GetInteractableColliders(LayerMask interactablesLayer)
    {
        return Physics2D.OverlapCircleAll(transform.position, detectionRadius, interactablesLayer);
    }

    /// <summary>
    /// 各モードに応じた更新処理（派生クラスで実装）
    /// </summary>
    public abstract void ModeUpdate();

    /// <summary>
    /// リソースのクリーンアップ処理（派生クラスで実装）
    /// </summary>
    public abstract void CleanupResources();

    /// <summary>
    /// シーン上に検出範囲を描画します（エディタ用）
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
