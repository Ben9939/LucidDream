using System.Collections;
using UnityEngine;

/// <summary>
/// ページめくりアニメーションを管理するクラス
/// </summary>
public class PageFlipController : MonoBehaviour
{
    [HideInInspector] public GameObject currentPage;  // 現在のページ
    [HideInInspector] public GameObject nextPage;     // 次のページ（右にめくる場合）
    [HideInInspector] public GameObject previousPage; // 前のページ（左にめくる場合）
    public float flipDuration = 1.0f; // ページめくりアニメーションの総時間
    public AnimationCurve flipCurve; // カスタムアニメーションカーブ

    private bool isAnimating = false; // アニメーションの多重起動を防止するフラグ

    /// <summary>
    /// 右方向にページをめくる処理
    /// </summary>
    /// <param name="onComplete">アニメーション完了時のコールバック</param>
    public void FlipToNextPage(System.Action onComplete)
    {
        if (!isAnimating && nextPage != null)
        {
            StartCoroutine(FlipPageAnimation(currentPage, nextPage, true, onComplete));
        }
    }

    /// <summary>
    /// 左方向にページをめくる処理
    /// </summary>
    /// <param name="onComplete">アニメーション完了時のコールバック</param>
    public void FlipToPreviousPage(System.Action onComplete)
    {
        if (!isAnimating && previousPage != null)
        {
            StartCoroutine(FlipPageAnimation(currentPage, previousPage, false, onComplete));
        }
    }

    private IEnumerator FlipPageAnimation(GameObject current, GameObject target, bool isRightFlip, System.Action onComplete)
    {
        isAnimating = true;
        target.SetActive(true);
        Canvas currentCanvas = current.GetComponent<Canvas>();
        Canvas targetCanvas = target.GetComponent<Canvas>();

        current.transform.SetSiblingIndex(1);
        target.transform.SetSiblingIndex(0);

        // ページの各面（右・左）を取得する
        var currentPageSide = isRightFlip ? current.transform.Find("RightPage").gameObject : current.transform.Find("LeftPage").gameObject;
        var targetPageSide = isRightFlip ? target.transform.Find("LeftPage").gameObject : target.transform.Find("RightPage").gameObject;
        var anotherTargetPageSide = isRightFlip ? target.transform.Find("RightPage").gameObject : target.transform.Find("LeftPage").gameObject;

        // ページの初期状態を設定する
        currentPageSide.SetActive(true);
        targetPageSide.SetActive(false);
        anotherTargetPageSide.SetActive(true);

        float halfDuration = flipDuration / 2f;
        float time = 0f;
        // **第一段階：現在のページを圧縮する**
        while (time <= halfDuration)
        {
            float normalizedTime = Mathf.Clamp01(time / halfDuration); // 0～1に正規化
            float scale = Mathf.Lerp(1, 0, normalizedTime); // スケールを 1 から 0 へ変化
            currentPageSide.transform.localScale = new Vector3(scale, 1, 1); // X軸方向に圧縮
            time += Time.deltaTime;
            yield return null;
        }

        // ページの順序を入れ替える
        current.transform.SetSiblingIndex(target.transform.GetSiblingIndex());

        currentPageSide.SetActive(false); // 現在のページを非表示にする
        targetPageSide.SetActive(true);   // ターゲットのページを表示する
        time = 0f;

        if (currentCanvas != null && targetCanvas != null)
        {
            currentCanvas.sortingOrder = 0;
            targetCanvas.sortingOrder = 1;
        }

        // **第二段階：ターゲットのページを展開する**
        while (time <= halfDuration)
        {
            float normalizedTime = Mathf.Clamp01(time / halfDuration);
            float scale = Mathf.Lerp(0, 1, normalizedTime); // スケールを 0 から 1 へ変化
            targetPageSide.transform.localScale = new Vector3(scale, 1, 1); // X軸方向に展開
            time += Time.deltaTime;
            yield return null;
        }

        // ページめくり完了：状態を更新する
        current.SetActive(false); // 現在のページを完全に非表示にする
        target.SetActive(true);   // ターゲットのページを完全に表示する
        isAnimating = false;      // アニメーション終了
        target.transform.SetSiblingIndex(0);
        onComplete?.Invoke(); // アニメーション完了後のコールバックを実行する
    }
}
