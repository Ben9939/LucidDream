using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ページの管理とページめくり操作を制御するクラス
/// </summary>
public class PageManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private GameObject rightButtonObj;
    [SerializeField] private GameObject leftButtonObj;
    private Button rightButton;
    private Button leftButton;
    private int currentIndex = 0;

    private PageFlipController pageFlipController;

    private void Start()
    {
        if (pages == null || pages.Count == 0)
        {
            Debug.LogWarning("Page list is empty. Please check the assigned pages!");
            return;
        }

        pageFlipController = GetComponent<PageFlipController>();

        rightButton = rightButtonObj.GetComponent<Button>();
        leftButton = leftButtonObj.GetComponent<Button>();

        rightButton.onClick.AddListener(() => FlipToNext());
        leftButton.onClick.AddListener(() => FlipToPrevious());

        UpdatePageReferences();
        UpdateButtonStates();
    }

    /// <summary>
    /// 次のページにめくる処理
    /// </summary>
    public void FlipToNext()
    {
        if (currentIndex < pages.Count - 1)
        {
            DisableButtons(); // ボタンを無効化する
            pageFlipController.FlipToNextPage(() =>
            {
                currentIndex++;
                UpdatePageReferences();
                UpdateButtonStates();
                EnableButtons(); // アニメーション終了後にボタンを有効化する
            });
        }
        else
        {
            Debug.Log("Reached the last page!");
        }
    }

    /// <summary>
    /// 前のページに戻る処理
    /// </summary>
    public void FlipToPrevious()
    {
        if (currentIndex > 0)
        {
            DisableButtons(); // ボタンを無効化する
            pageFlipController.FlipToPreviousPage(() =>
            {
                currentIndex--;
                UpdatePageReferences();
                UpdateButtonStates();
                EnableButtons(); // アニメーション終了後にボタンを有効化する
            });
        }
        else
        {
            Debug.Log("Reached the first page!");
        }
    }

    /// <summary>
    /// ボタンを無効化する
    /// </summary>
    private void DisableButtons()
    {
        rightButton.interactable = false;
        leftButton.interactable = false;
    }

    /// <summary>
    /// ボタンの状態を更新する
    /// </summary>
    private void EnableButtons()
    {
        rightButton.interactable = currentIndex < pages.Count - 1;
        leftButton.interactable = currentIndex > 0;
    }

    /// <summary>
    /// 現在のページの参照を更新する
    /// </summary>
    private void UpdatePageReferences()
    {
        pageFlipController.currentPage = pages[currentIndex];
        pageFlipController.nextPage = (currentIndex < pages.Count - 1) ? pages[currentIndex + 1] : null;
        pageFlipController.previousPage = (currentIndex > 0) ? pages[currentIndex - 1] : null;
    }

    /// <summary>
    /// ボタンの状態を更新する
    /// </summary>
    private void UpdateButtonStates()
    {
        rightButton.interactable = currentIndex < pages.Count - 1;
        leftButton.interactable = currentIndex > 0;
    }
}
