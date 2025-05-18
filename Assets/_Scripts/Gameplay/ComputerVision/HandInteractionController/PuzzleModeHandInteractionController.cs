using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; // 必要に応じて

/// <summary>
/// パズルモード用の手操作クラスです。
/// 基底クラスのリソースを設定・初期化し、パズルのドラッグ、配置、回転、境界処理を実装します。
/// </summary>
public class PuzzleModeHandInteractionController : HandInteractionControllerBase
{
    // === 基底クラス用のリソース ===
    [Header("Base Class Resources")]
    [SerializeField] private GameObject myHandTracker;       // handTracker に注入するオブジェクト
    [SerializeField] private List<Sprite> myHandPointSprite;   // handPointSprite に注入するスプライト
    [SerializeField] private float myDetectionRadius;          // detectionRadius に注入する値

    // === パズルモード固有のフィールド ===
    [Header("Puzzle Mode Fields")]
    [SerializeField] private List<GameObject> puzzleList;
    [SerializeField] private GameObject workbench;
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private GameObject boundary;
    [SerializeField] private Vector2 boundaryMin;
    [SerializeField] private Vector2 boundaryMax;

    private GameObject currentDraggingObject = null;
    private Stack<GameObject> puzzleStack = new Stack<GameObject>();
    private bool isPickingPuzzle = false;
    private bool isCollisionHandling = false;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.5f;

    public event Action isAllPuzzlePlaced; // 全パズル配置完了時のイベント

    /// <summary>
    /// PuzzleStack プロパティです。
    /// </summary>
    public Stack<GameObject> PuzzleStack
    {
        get { return puzzleStack; }
    }

    protected override void Awake()
    {
        // まず基底クラスの Awake() を呼び出す
        base.Awake();

        // 基底クラスのフィールドに値を代入して初期化する
        handTracker = myHandTracker;
        handPointSprite = myHandPointSprite;
        detectionRadius = myDetectionRadius;
    }

    /// <summary>
    /// パズルの初期化処理を行います。
    /// パズルリストの設定と、手とパズル間の衝突設定を実施します。
    /// </summary>
    /// <param name="puzzles">初期化するパズルオブジェクトのリスト</param>
    public void PuzzleInstance(List<GameObject> puzzles)
    {
        this.puzzleList = puzzles;
        if (!workbench)
            workbench = GameObject.Find("Workbench29X29");

        Collider2D handPointCollider = GetComponent<Collider2D>();
        foreach (var item in puzzleList)
        {
            Collider2D puzzleCollider = item.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(handPointCollider, puzzleCollider, false);
        }
    }

    /// <summary>
    /// モード更新処理です。
    /// 手の動作、位置更新、オブジェクトのドラッグ更新、パズルの配置・ドロップ、境界外チェックを行います。
    /// </summary>
    public override void ModeUpdate()
    {
        UpdateHandMotion();
        UpdateHandPosition();
        ObjectDragUpdate();

        Collider2D[] colliders = GetInteractableColliders(interactablesLayer);
        bool currentHandState = handTracker.GetComponent<HandTracking>().handState;

        if (currentHandState != lastHandState)
        {
            if (!currentHandState) // 手が閉じたとき：パズルを取得する
            {
                HandlePuzzleCollisionsAsync(colliders);
            }
            else // 手が開いたとき：パズルをドロップする
            {
                HandleDrop();
            }
            lastHandState = currentHandState;
        }

        CheckPuzzleOutOfBounds();
    }

    /// <summary>
    /// パズルとの衝突判定処理（async 実装）です。
    /// 衝突しているパズルオブジェクトに対してドラッグ開始処理およびダブルクリック回転チェックを行います。
    /// </summary>
    /// <param name="colliders">衝突判定対象のコライダー配列</param>
    private async void HandlePuzzleCollisionsAsync(Collider2D[] colliders)
    {
        if (isCollisionHandling)
            return;
        isCollisionHandling = true;

        foreach (Collider2D collider in colliders)
        {
            if (puzzleList.Contains(collider.gameObject))
            {
                GameObject puzzleObject = collider.gameObject;
                HandleDrag(puzzleObject);

                // ワークベンチに未配置の場合、ダブルクリックで回転処理をチェック
                if (!puzzleStack.Contains(puzzleObject))
                {
                    HandleDoubleClickToRotate(puzzleObject);
                }
            }
        }

        // 必要に応じて待機処理を追加可能（例： await Task.Delay(500);）
        isCollisionHandling = false;
    }

    /// <summary>
    /// ドラッグ中のパズルオブジェクトの位置を更新します。
    /// </summary>
    private void ObjectDragUpdate()
    {
        if (currentDraggingObject != null)
        {
            currentDraggingObject.transform.position = transform.position;
        }
    }

    /// <summary>
    /// パズルが境界外に出た場合、境界内に戻す処理を行います。
    /// </summary>
    private void CheckPuzzleOutOfBounds()
    {
        foreach (var puzzleObject in puzzleList)
        {
            if (puzzleObject != null)
            {
                Vector3 position = puzzleObject.transform.position;
                if (position.x < boundaryMin.x || position.x > boundaryMax.x ||
                    position.y < boundaryMin.y || position.y > boundaryMax.y)
                {
                    position.x = Mathf.Clamp(position.x, boundaryMin.x, boundaryMax.x);
                    position.y = Mathf.Clamp(position.y, boundaryMin.y, boundaryMax.y);
                    puzzleObject.transform.position = position;
                    Debug.Log($"Moved puzzle {puzzleObject.name} into bounds.");
                }
            }
        }
    }

    /// <summary>
    /// パズルオブジェクトのドラッグ開始処理を行います。
    /// </summary>
    /// <param name="puzzleObject">対象のパズルオブジェクト</param>
    private void HandleDrag(GameObject puzzleObject)
    {
        if (currentDraggingObject == null && !isPickingPuzzle)
        {
            isPickingPuzzle = true;
            if (puzzleStack.Contains(puzzleObject))
            {
                HandleRemovePuzzle(puzzleObject);
            }
            currentDraggingObject = puzzleObject;
        }
    }

    /// <summary>
    /// ワークベンチ上からパズルを除去し、物理演算状態を復元します。
    /// </summary>
    /// <param name="puzzleObject">対象のパズルオブジェクト</param>
    private void HandleRemovePuzzle(GameObject puzzleObject)
    {
        if (puzzleStack.Count > 0 && puzzleStack.Contains(puzzleObject))
        {
            puzzleStack.Pop();
            ResetRigidbodyState(puzzleObject, true);
            Debug.Log($"Removed puzzle {puzzleObject.name} from workbench and restored physics.");
        }
    }

    /// <summary>
    /// ワークベンチ上にパズルを配置する処理です。
    /// パズルオブジェクトがワークベンチ上にある場合、スタックに追加し固定します。
    /// </summary>
    /// <param name="puzzleObject">対象のパズルオブジェクト</param>
    private void HandlePlacePuzzle(GameObject puzzleObject)
    {
        Collider2D collider = Physics2D.OverlapBox(
            workbench.transform.position,
            new Vector2(2, 2),
            0,
            interactablesLayer
        );
        if (collider != null && collider.gameObject == puzzleObject)
        {
            if (!puzzleStack.Contains(puzzleObject))
            {
                puzzleStack.Push(puzzleObject);
                ResetRigidbodyState(puzzleObject, false);
                puzzleObject.transform.position = workbench.transform.position;
                Debug.Log($"Placed puzzle {puzzleObject.name} on workbench and fixed it.");
                isAllPuzzlePlaced?.Invoke();
            }
        }
    }

    /// <summary>
    /// ドラッグ終了時にパズルを配置または状態を復元する処理です。
    /// </summary>
    private void HandleDrop()
    {
        if (currentDraggingObject != null)
        {
            if (!puzzleStack.Contains(currentDraggingObject))
            {
                HandlePlacePuzzle(currentDraggingObject);
            }
            else
            {
                ResetRigidbodyState(currentDraggingObject, true);
            }
            isPickingPuzzle = false;
            Debug.Log($"Dropped puzzle {currentDraggingObject.name} and restored state.");
            currentDraggingObject = null;
        }
    }

    /// <summary>
    /// パズルオブジェクトの Rigidbody 状態をリセットします。
    /// enablePhysics が true の場合は物理演算を有効にし、false の場合は無効にします。
    /// </summary>
    /// <param name="puzzleObject">対象のパズルオブジェクト</param>
    /// <param name="enablePhysics">物理演算を有効にするかどうか</param>
    private void ResetRigidbodyState(GameObject puzzleObject, bool enablePhysics)
    {
        Rigidbody2D rb = puzzleObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = !enablePhysics;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        Debug.Log($"Reset Rigidbody state for puzzle {puzzleObject.name}. Physics enabled: {enablePhysics}");
    }

    /// <summary>
    /// ダブルクリックでパズルオブジェクトを回転させる処理です。
    /// 連続クリック時間が doubleClickThreshold 以下の場合、90度回転します。
    /// </summary>
    /// <param name="puzzleObject">対象のパズルオブジェクト</param>
    private void HandleDoubleClickToRotate(GameObject puzzleObject)
    {
        float currentTime = Time.time;
        if (currentTime - lastClickTime < doubleClickThreshold)
        {
            puzzleObject.transform.Rotate(0, 0, 90);
            Debug.Log($"Rotated puzzle {puzzleObject.name} by 90 degrees on double-click.");
            lastClickTime = 0f;
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    /// <summary>
    /// 基底クラスで定義されたリソースのクリーンアップ処理です。
    /// </summary>
    public override void CleanupResources()
    {
        if (transform != null)
        {
            transform.position = Vector3.zero;
        }
    }
}