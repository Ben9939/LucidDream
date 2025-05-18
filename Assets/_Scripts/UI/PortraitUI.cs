using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 立ち絵のUIを管理するクラス
/// </summary>
public class PortraitUI : MonoBehaviour
{
    // ※必要に応じて、Prefab 内の基本立ち絵表示用 Image を利用する場合、以下のコメントを解除してください
    // [SerializeField] private Image portraitImage;

    // 講話アニメーションを制御する Animator。Animator には "IsTalking" パラメータを設定してください。
    [SerializeField] private Animator animator;

    /// <summary>
    /// 講話状態を設定する
    /// </summary>
    /// <param name="isTalking">講話中の場合は true、停止の場合は false</param>
    public void SetTalking(bool isTalking)
    {
        if (animator != null)
        {
            animator.SetBool("IsTalking", isTalking);
        }
    }

    // 基本立ち絵画像を設定する場合のメソッド（必要に応じて利用）
    /*
    public void SetPortraitSprite(Sprite sprite)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = sprite;
            portraitImage.SetNativeSize();
        }
    }
    */
}
