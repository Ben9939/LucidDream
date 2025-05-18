using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

/// <summary>
/// ツールチップ付きボタンの挙動を管理するクラス
/// </summary>
public class ButtonWithTooltip : MonoBehaviour
{
    [Header("Button Sprites")]
    public Sprite normalSprite; // 通常状態のボタン画像
    public Sprite hoverSprite;  // ホバー状態のボタン画像

    [Header("Tooltip Settings")]
    public GameObject tooltipText; // ツールチップの GameObject

    [Header("Mutual Exclusive Buttons")]
    // public GameObject otherButton; // 互いに排他的な別のボタン（必要に応じて）

    // ボタンの Image コンポーネント
    private Image buttonImage;
    // オフセット（camelCase を使用）
    private float offsetX = -1.5f;

    public static ButtonWithTooltip Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ボタン画像とツールチップの初期化
        buttonImage = GetComponent<Image>();
        if (tooltipText != null)
        {
            tooltipText.SetActive(false);
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理（ツールチップ位置の更新等）
    /// </summary>
    public void HandleUpdate()
    {
        if (tooltipText != null && tooltipText.activeSelf)
        {
            UpdateTooltipPosition();
        }
        MenuButtonHint();
    }

    /// <summary>
    /// マウスオーバー時にボタン画像を変更し、ツールチップを表示する
    /// </summary>
    private void MenuButtonHint()
    {
        if (Helper.IsOverUi(gameObject))
        {
            buttonImage.sprite = hoverSprite;
            tooltipText.SetActive(true);
        }
        else
        {
            buttonImage.sprite = normalSprite;
            tooltipText.SetActive(false);
        }
    }

    /// <summary>
    /// ツールチップの位置をマウス位置に更新する
    /// </summary>
    private void UpdateTooltipPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));
            tooltipText.transform.position = new Vector3(worldPosition.x + offsetX, worldPosition.y, tooltipText.transform.position.z);
        }
    }
}
