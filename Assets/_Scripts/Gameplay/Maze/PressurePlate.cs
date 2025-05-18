using System.Collections;
using UnityEngine;

/// <summary>
/// PressurePlateType 列挙型は、圧力板が回転する軸の種類を示します。
/// Horizontal は Y 軸（水平回転）、Vertical は X 軸（垂直回転）を表します。
/// </summary>
public enum PressurePlateType
{
    Horizontal, // Horizontal rotation (around Y axis)
    Vertical    // Vertical rotation (around X axis)
}

/// <summary>
/// PressurePlate クラスは、プレイヤーが圧力板に乗ったかどうかを検出し、
/// 対象オブジェクトを回転させるとともに、プレイヤーの移動を一時停止する処理を行います。
/// また、回転完了後に障害物の生成などを行います。
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Header("Pressure Plate Settings")]
    [SerializeField] private Sprite pressedSprite;      // Pressed state sprite
    [SerializeField] private Sprite unpressedSprite;      // Unpressed state sprite
    [SerializeField] private PressurePlateType plateType; // Pressure plate type (Horizontal or Vertical)
    [SerializeField] private GameObject targetObject;     // Target 3D object to rotate
    [SerializeField] private float rotationDuration = 0.5f; // Rotation duration in seconds
    [SerializeField] private LayerMask playerLayer;       // Layer of the player (for detection)
    [SerializeField] private Vector2 detectionSize = new Vector2(1f, 1f); // Size of detection area
    [SerializeField]
    private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Puzzle generator reference (optional)
    public ThreeDimensionalPuzzleGenerator puzzleGenerator;

    private SpriteRenderer spriteRenderer;
    private bool isPressed = false;  // 圧力板が押されているか
    private bool isRotating = false; // 回転中か

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && unpressedSprite != null)
        {
            spriteRenderer.sprite = unpressedSprite;
        }
    }

    private void Update()
    {
        if (isRotating)
            return;

        bool playerOnPlate = CheckPlayerOnPlate();
        if (playerOnPlate && !isPressed)
        {
            ActivatePressurePlate();
        }
        else if (!playerOnPlate && isPressed)
        {
            DeactivatePressurePlate();
        }
    }

    /// <summary>
    /// プレイヤーが圧力板上にいるかどうかを検出します。
    /// </summary>
    /// <returns>プレイヤーがいる場合 true</returns>
    private bool CheckPlayerOnPlate()
    {
        Vector2 detectionCenter = (Vector2)transform.position;
        Collider2D collider = Physics2D.OverlapBox(detectionCenter, detectionSize, 0f, playerLayer);
        return collider != null;
    }

    /// <summary>
    /// プレイヤーが圧力板に乗ったときの処理を行い、対象オブジェクトの回転を開始します。
    /// </summary>
    private void ActivatePressurePlate()
    {
        isPressed = true;
        if (spriteRenderer != null && pressedSprite != null)
        {
            spriteRenderer.sprite = pressedSprite;
        }
        PlayerController.Instance?.DisablePlayerControl();
        if (targetObject != null)
        {
            RotateTargetObject();
        }
    }

    /// <summary>
    /// プレイヤーが圧力板から離れたときの処理を行い、圧力板の状態を復元します。
    /// </summary>
    private void DeactivatePressurePlate()
    {
        isPressed = false;
        if (spriteRenderer != null && unpressedSprite != null)
        {
            spriteRenderer.sprite = unpressedSprite;
        }
    }

    /// <summary>
    /// 対象オブジェクトの回転処理を開始します。
    /// </summary>
    private void RotateTargetObject()
    {
        if (isRotating)
        {
            Debug.LogWarning("Rotation already in progress!");
            return;
        }

        Vector3 rotationAxis = Vector3.zero;
        switch (plateType)
        {
            case PressurePlateType.Horizontal:
                rotationAxis = Vector3.up;
                break;
            case PressurePlateType.Vertical:
                rotationAxis = Vector3.right;
                break;
        }
        StartCoroutine(RotateObject(targetObject.transform, rotationAxis, 90f, rotationDuration));
    }

    /// <summary>
    /// 対象オブジェクトを指定の軸、角度、時間で回転させるコルーチンです。
    /// </summary>
    /// <param name="target">回転対象の Transform</param>
    /// <param name="axis">回転軸</param>
    /// <param name="angle">回転角度（度）</param>
    /// <param name="duration">回転にかかる時間（秒）</param>
    /// <returns>コルーチン</returns>
    /// 
    private IEnumerator RotateObject(Transform target, Vector3 axis, float angle, float duration)
    {
        isRotating = true;
        AudioManager.Instance.PlaySE(SESoundData.SE.Mechanism);
        Vector3 center = GetObjectCenter(target);
        Quaternion startRotation = target.rotation;
        Vector3 startPosition = target.position;
        Vector3 offset = startPosition - center;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 线性 t 值
            float t = Mathf.Clamp01(elapsed / duration);
            // 通过 AnimationCurve 实现非线性插值
            float easedT = rotationCurve.Evaluate(t); // rotationCurve 可在 Inspector 中设定，默认设为 EaseInOut(0,0,1,1)
            float currentAngle = easedT * angle;
            Quaternion rotationQuat = Quaternion.AngleAxis(currentAngle, axis);
            target.position = center + rotationQuat * offset;
            target.rotation = rotationQuat * startRotation;
            yield return null;
        }

        // 最终确保完全旋转
        Quaternion finalRotation = Quaternion.AngleAxis(angle, axis) * startRotation;
        target.position = center + Quaternion.AngleAxis(angle, axis) * offset;
        target.rotation = finalRotation;

        isRotating = false;
        PlayerController.Instance?.EnablePlayerControl();

        if (puzzleGenerator != null)
        {
            puzzleGenerator.VisualizeObstacles();
        }
    }

    //private IEnumerator RotateObject(Transform target, Vector3 axis, float angle, float duration)
    //{
    //    isRotating = true;

    //    Vector3 center = GetObjectCenter(target);
    //    Quaternion startRotation = target.rotation;
    //    Vector3 startPosition = target.position;
    //    Vector3 offset = startPosition - center;



    //    float elapsed = 0f;
    //    while (elapsed < duration)
    //    {
    //        elapsed += Time.deltaTime;
    //        float t = Mathf.Clamp01(elapsed / duration);
    //        float currentAngle = t * angle;
    //        Quaternion rotationQuat = Quaternion.AngleAxis(currentAngle, axis);
    //        target.position = center + rotationQuat * offset;
    //        target.rotation = rotationQuat * startRotation;
    //        yield return null;
    //    }

    //    // 最終補正
    //    Quaternion finalRotation = Quaternion.AngleAxis(angle, axis) * startRotation;
    //    target.position = center + Quaternion.AngleAxis(angle, axis) * offset;
    //    target.rotation = finalRotation;

    //    isRotating = false;
    //    PlayerController.Instance?.EnablePlayerControl();

    //    if (puzzleGenerator != null)
    //    {
    //        puzzleGenerator.VisualizeObstacles();
    //    }
    //}

    /// <summary>
    /// 対象オブジェクトの中心点を取得します（子オブジェクトを含む）。
    /// </summary>
    /// <param name="target">対象の Transform</param>
    /// <returns>中心座標</returns>
    private Vector3 GetObjectCenter(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return target.position;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.center;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, detectionSize);
    }
}
