using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NPC 対話用の UI を管理するクラス
/// </summary>
public class NpcDialogUI : MonoBehaviour, IDialogUI
{
    [Header("Dialog Box")]
    [SerializeField] private GameObject npcDialogBox;
    [SerializeField] private Text npcDialogText;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Portrait and Dialog Container")]
    [SerializeField] private Transform portraitContainer;
    [SerializeField] private Transform dialogBoxRect; // 対話ボックスの反転用
    [SerializeField] private Image CG;

    [Header("Speaker Data")]
    [SerializeField] private List<SpeakerData> speakerDatas;

    // 動的生成した選択肢テキストのリスト
    private List<Text> optionTextUI = new List<Text>();

    /// <summary>
    /// 対話 UI を初期化し、文字をクリアおよび選択肢パネルを非表示にする
    /// </summary>
    public void InitUI()
    {
        npcDialogBox.SetActive(true);
        npcDialogText.text = "";
        HideOptions();
    }

    /// <summary>
    /// 指定されたスピーカーの立ち絵を表示する
    /// （生成されていない場合は SpeakerData.Initialize を呼び出す）
    /// </summary>
    public void ShowSpeaker(SpeakerName speaker)
    {
        if (speaker == SpeakerName.None)
            return;

        SpeakerData data = speakerDatas.Find(s => s.speakerName == speaker);
        if (data != null)
        {
            if (data.portraitObject == null && portraitContainer != null)
            {
                data.Initialize(portraitContainer);
            }
            if (data.portraitObject != null)
            {
                data.portraitObject.SetActive(true);
            }
        }
        FlipDialogBox(speaker);
    }

    /// <summary>
    /// 対話ボックスの反転処理
    /// スピーカーが Player の場合は正方向、その他は反転する
    /// </summary>
    private void FlipDialogBox(SpeakerName speaker)
    {
        if (dialogBoxRect == null) return;
        dialogBoxRect.localScale = (speaker == SpeakerName.Player)
            ? new Vector3(1, 1, 1)
            : new Vector3(-1, 1, 1);
    }

    /// <summary>
    /// 指定されたスピーカーの立ち絵を非表示にする
    /// </summary>
    public void HideSpeaker(SpeakerName speaker)
    {
        if (speaker == SpeakerName.None)
            return;
        SpeakerData data = speakerDatas.Find(s => s.speakerName == speaker);
        if (data != null && data.portraitObject != null)
        {
            data.portraitObject.SetActive(false);
        }
    }

    /// <summary>
    /// スピーカーの講話状態を設定する（例： PortraitUI の SetTalking を呼ぶ）
    /// </summary>
    public void SetSpeakerTalking(SpeakerName speaker, bool isTalking)
    {
        if (speaker == SpeakerName.None)
            return;
        SpeakerData data = speakerDatas.Find(s => s.speakerName == speaker);
        if (data != null && data.portraitUI != null)
        {
            data.portraitUI.SetTalking(isTalking);
        }
    }

    /// <summary>
    /// スピーカー名を表示する（例：「名前: 」の形式）
    /// </summary>
    public void SetSpeakerName(SpeakerName speaker)
    {
        if (speaker == SpeakerName.None)
            return;
        SpeakerData data = speakerDatas.Find(s => s.speakerName == speaker);
        npcDialogText.text = data.unitData.unitName.ToString() + ":\n";
    }

    /// <summary>
    /// 対話テキストを追加表示する
    /// </summary>
    public void ShowLineText(string text)
    {
        npcDialogText.text += text;
    }

    /// <summary>
    /// CG（背景画像）を表示する（フェードイン）
    /// </summary>
    public void ShowCg(Sprite newSprite)
    {
        if (CG != null && newSprite != null)
        {
            StartCoroutine(FadeInCG(newSprite));
        }
    }

    /// <summary>
    /// CG（背景画像）を非表示にする（フェードアウト）
    /// </summary>
    public void HideCg()
    {
        if (CG != null && CG.sprite != null)
        {
            StartCoroutine(FadeOutCG());
        }
    }

    /// <summary>
    /// CG のフェードイン処理（透明から不透明へ）
    /// </summary>
    public IEnumerator FadeInCG(Sprite newSprite)
    {
        if (newSprite == null)
            yield break;

        CG.sprite = newSprite;
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 0.05f;
            Color color = CG.color;
            color.a = alpha;
            CG.color = color;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// CG のフェードアウト処理（不透明から透明へ）
    /// </summary>
    public IEnumerator FadeOutCG()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= 0.05f;
            Color color = CG.color;
            color.a = alpha;
            CG.color = color;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// 対話テキストをクリアする
    /// </summary>
    public void ClearLineText()
    {
        npcDialogText.text = "";
    }

    /// <summary>
    /// 分岐選択肢を動的生成して表示する
    /// </summary>
    public void ShowOptions(List<string> options)
    {
        optionsPanel.SetActive(true);
        // 既存の選択肢を全削除
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        optionTextUI.Clear();

        RectTransform panelRect = optionsPanel.GetComponent<RectTransform>();
        float panelWidth = panelRect.rect.width;
        float spacing = 10f;
        float optionHeight = 60f;
        float totalHeight = options.Count * (optionHeight + spacing);
        panelRect.sizeDelta = new Vector2(panelWidth, Mathf.Min(totalHeight, 400));

        // VerticalLayoutGroup を使用して選択肢を配置する
        VerticalLayoutGroup layoutGroup = optionsPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = optionsPanel.AddComponent<VerticalLayoutGroup>();
        }
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = spacing;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);

        for (int i = 0; i < options.Count; i++)
        {
            GameObject newOption = new GameObject($"Option_{i}", typeof(RectTransform), typeof(Text));
            newOption.transform.SetParent(optionsPanel.transform, false);

            RectTransform rt = newOption.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(panelWidth - 20, optionHeight);

            Text optionText = newOption.GetComponent<Text>();
            optionText.font = npcDialogText.font; // 対話テキストと同じフォントを使用
            optionText.fontSize = 30;
            optionText.alignment = TextAnchor.MiddleCenter;
            optionText.color = Color.white;
            optionText.text = options[i];

            optionTextUI.Add(optionText);
        }

        int initialSelectedIndex = 0;
        UpdateOptionText(initialSelectedIndex);
    }

    /// <summary>
    /// 選択中の選択肢に "> " を付加して更新する
    /// </summary>
    public void UpdateOptionText(int selectedOptionIndex)
    {
        for (int i = 0; i < optionTextUI.Count; i++)
        {
            string orig = optionTextUI[i].text;
            if (orig.StartsWith("> "))
            {
                orig = orig.Substring(2);
            }
            optionTextUI[i].text = (i == selectedOptionIndex) ? "> " + orig : orig;
        }
    }

    /// <summary>
    /// 選択肢パネルを非表示にし、動的生成した選択肢を削除する
    /// </summary>
    public void HideOptions()
    {
        optionsPanel.SetActive(false);
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        optionTextUI.Clear();
    }

    /// <summary>
    /// 対話終了時の処理
    /// 対話ボックスを非表示にし、選択肢をクリアするとともに、すべての立ち絵を破棄してリソースを解放する
    /// </summary>
    public void OnDialogEnd()
    {
        npcDialogBox.SetActive(false);
        HideOptions();

        foreach (SpeakerData data in speakerDatas)
        {
            if (data.portraitObject != null)
            {
                Destroy(data.portraitObject);
                data.portraitObject = null;
            }
        }
    }
}
