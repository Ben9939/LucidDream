using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using System.Collections;
using static SceneMapManager;

public enum ColorMode
{
    Monochrome,
    Color,
}

/// <summary>
/// プレイヤーの移動と操作を管理するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    
    public float moveSpeed;
    public bool isMoving;
    public PlayerDataSO playerData;

    private Vector2 input;
    private Animator animator;
    public LayerMask solidObjectsLayer;
    public LayerMask interactablesLayer;
    public LayerMask sceneBounds;
    private Vector2 detectionDistance = new Vector2(0.5f, 0.5f);
    private bool canMove = true;
    public UnityAction resetPlayerAction;

    [SerializeField] GameObject background;
    private bool dayTime = false;

    public static PlayerController Instance { get; private set; }

    /// <summary>
    /// Awake メソッド：シングルトンの設定とイベントの購読
    /// </summary>
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        animator = GetComponent<Animator>();

        TransitionController.Instance.isOnTransitionStart += DisablePlayerControl;
        TransitionController.Instance.isOnTransitionEnd += EnablePlayerControl;

        // PlayerDataSO の位置変化イベントを購読
        if (playerData != null)
            playerData.OnPositionChanged += OnDataPositionChanged;

        resetPlayerAction += ResetPlayer;
        GameEventManager.StartListening(GameEventName.ResetGame, resetPlayerAction);
    }

    private void Start()
    {
        transform.position = playerData.CurrentPosition;
    }

    /// <summary>
    /// プレイヤーの初期位置を設定し、フリーロームシーンに遷移する
    /// </summary>
    public void InitializePlayerPosition()
    {
        transform.position = playerData.CurrentPosition;
        SceneMapManager.Instance.SetFreeRoamScene();
    }

    private void OnDestroy()
    {
        TransitionController.Instance.isOnTransitionStart -= DisablePlayerControl;
        TransitionController.Instance.isOnTransitionEnd -= EnablePlayerControl;

        if (playerData != null)
            playerData.OnPositionChanged -= OnDataPositionChanged;
    }

    /// <summary>
    /// プレイヤーの操作を無効化する
    /// </summary>
    public void DisablePlayerControl()
    {
        canMove = false;
    }

    /// <summary>
    /// プレイヤーの操作を有効化する
    /// </summary>
    public void EnablePlayerControl()
    {
        canMove = true;
    }

    /// <summary>
    /// 現在のプレイヤーの位置を取得する
    /// </summary>
    public Vector2 GetPlayerPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// 目的地が歩行可能か判定する
    /// </summary>
    private bool IsWalkable(Vector3 targetPos)
    {
        Vector2 targetPos2D = new Vector2(targetPos.x, targetPos.y);
        Debug.DrawLine(targetPos2D, targetPos2D + detectionDistance, Color.red, 1f);
        Debug.DrawLine(targetPos2D, targetPos2D - detectionDistance, Color.red, 1f);

        if (Physics2D.OverlapCircle(targetPos2D, 0.1f, solidObjectsLayer | interactablesLayer) != null)
        {
            return false;
        }

        if (Physics2D.OverlapCircle(targetPos2D, 0.1f, sceneBounds) != null)
        {
            Collider2D collider = Physics2D.OverlapCircle(targetPos2D, 0.1f, sceneBounds);
            SceneMapManager.Instance.SceneSwitchEvent(collider.gameObject.name);
            return false;
        }

        return true;
    }

    /// <summary>
    /// プレイヤーの操作を処理する
    /// </summary>
    public void HandleUpdate()
    {
        // キー T を押下し、"Candle" アイテムを所持している場合、特定シーン境界でライトアップを実行
        if (Input.GetKeyDown(KeyCode.T) && ItemManager.Instance.HasItem("Candle") &&
            (playerData.CurrentBoundary == SceneBoundaryName.Level7_1 ||
             playerData.CurrentBoundary == SceneBoundaryName.Level7_2 ||
             playerData.CurrentBoundary == SceneBoundaryName.Level7_3))
        {
            HnedleLightUp();
        }
        if (canMove && !isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // 横方向入力がある場合、縦方向入力を無視
            if (input.x != 0)
                input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                Vector3 targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }

            animator.SetBool("isMoving", isMoving);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Interact();
            }
        }
    }

    /// <summary>
    /// 近くの対話対象と対話を開始する
    /// </summary>
    void Interact()
    {
        Collider2D collider = GetInteractTarget();
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    /// <summary>
    /// 対話対象のコライダーを取得する
    /// </summary>
    private Collider2D GetInteractTarget()
    {
        Vector3 facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        Vector3 interactPos = transform.position + facingDir;
        return Physics2D.OverlapCircle(interactPos, 0.1f, interactablesLayer);
    }

    /// <summary>
    /// 指定位置へ移動するコルーチン
    /// </summary>
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        AudioManager.Instance.PlaySE(SESoundData.SE.Move);
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        AudioManager.Instance.StopSE();
        transform.position = targetPos;
        // PlayerDataSO の位置を更新
        playerData.CurrentPosition = transform.position;
        isMoving = false;
    }

    /// <summary>
    /// PlayerDataSO の位置変化イベントのコールバック
    /// </summary>
    private void OnDataPositionChanged(Vector2 newPos)
    {
        if ((Vector2)transform.position != newPos)
        {
            transform.position = newPos;
        }
    }

    /// <summary>
    /// プレイヤーの位置を設定し、PlayerDataSO に同期する
    /// </summary>
    public void SetPlayerPosition(Vector2 playerPosition)
    {
        transform.position = playerPosition;
        playerData.CurrentPosition = playerPosition;
    }

    /// <summary>
    /// カラーモードに応じたアニメーションの色設定を変更する
    /// </summary>
    public void SetColor(ColorMode colorMode)
    {
        if (colorMode == ColorMode.Monochrome)
            animator.SetBool("isBlackWhite", true);
        else
            animator.SetBool("isBlackWhite", false);
    }

    /// <summary>
    /// ライトアップ処理を開始する
    /// </summary>
    public void HnedleLightUp()
    {
        StartCoroutine(PlaybackgroundDirector());
    }

    /// <summary>
    /// キャンドル使用時の処理を開始する
    /// </summary>
    public void UsedCandle()
    {
        StartCoroutine(UsedCandleCoroutine());
    }

    private IEnumerator UsedCandleCoroutine()
    {
        double halfDuration = background.GetComponent<PlayableDirector>().duration / 2;
        background.GetComponent<PlayableDirector>().initialTime = 0;
        background.GetComponent<PlayableDirector>().Play();
        yield return new WaitForSeconds((float)halfDuration);
        background.GetComponent<PlayableDirector>().Pause();
        background.GetComponent<PlayableDirector>().time = halfDuration;
        background.GetComponent<PlayableDirector>().initialTime = halfDuration;
        Debug.Log("Night");
        dayTime = false;
    }

    private IEnumerator PlaybackgroundDirector()
    {
        double halfDuration = background.GetComponent<PlayableDirector>().duration / 2;
        if (!dayTime)
        {
            background.GetComponent<PlayableDirector>().initialTime = halfDuration;
            background.GetComponent<PlayableDirector>().Play();
            yield return new WaitForSeconds((float)halfDuration);
            background.GetComponent<PlayableDirector>().Pause();
            background.GetComponent<PlayableDirector>().time = 0;
            background.GetComponent<PlayableDirector>().initialTime = 0;
            Debug.Log("Day");
            dayTime = true;
        }
        else
        {
            background.GetComponent<PlayableDirector>().initialTime = 0;
            background.GetComponent<PlayableDirector>().Play();
            yield return new WaitForSeconds((float)halfDuration);
            background.GetComponent<PlayableDirector>().Pause();
            background.GetComponent<PlayableDirector>().time = halfDuration;
            background.GetComponent<PlayableDirector>().initialTime = halfDuration;
            Debug.Log("Night");
            dayTime = false;
        }
    }

    /// <summary>
    /// プレイヤーデータをリセットする
    /// </summary>
    public void ResetPlayer()
    {
        playerData.ResetData();
    }

    public void OnApplicationQuit()
    {
        ResetPlayer();
        GameEventManager.StopListening(GameEventName.ResetGame, resetPlayerAction);
        resetPlayerAction -= ResetPlayer;
    }
}
