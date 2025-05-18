using UnityEngine;

/// <summary>
/// Puzzle クラスは、パズルの表示用スプライトを切り替える処理を行います。
/// </summary>
public class Puzzle : MonoBehaviour
{
    [SerializeField] private Sprite puzzleSprite; // Puzzle モード用のスプライト
    [SerializeField] private Sprite itemSprite;     // Inventory 用のスプライト

    /// <summary>
    /// 指定されたスプライトタイプに応じてスプライトを切り替えます。
    /// </summary>
    /// <param name="spriteType">切り替えるスプライトタイプ</param>
    public void SwitchSprite(PuzzleSpriteType spriteType)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (spriteType == PuzzleSpriteType.PuzzleSprite)
            {
                spriteRenderer.sprite = puzzleSprite;
            }
            else if (spriteType == PuzzleSpriteType.ItemSprite)
            {
                spriteRenderer.sprite = itemSprite;
            }
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on this GameObject!");
        }
    }
}

/// <summary>
/// スプライトの種類を定義する列挙型です。
/// </summary>
public enum PuzzleSpriteType
{
    PuzzleSprite, // Puzzle モード用
    ItemSprite    // Inventory 用
}
