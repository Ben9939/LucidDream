using UnityEngine;

/// <summary>
/// スプラッシュ画面で表示するデータを管理するScriptableObjectです。  
/// ・表示するメッセージと、（あれば）優先的に表示する画像を保持します。
/// </summary>
[CreateAssetMenu(menuName = "UI/SplashDataSO")]
public class SplashDataSO : ScriptableObject
{
    [Tooltip("表示する文字メッセージ（任意、空欄可）")]
    [TextArea]
    public string message;

    [Tooltip("表示する画像（設定されている場合、画像が優先して表示されます）")]
    public Sprite splashSprite;
}

//using UnityEngine;

//[CreateAssetMenu(menuName = "UI/SplashDataSO")]
//public class SplashDataSO: ScriptableObject
//{
//    [Tooltip("顯示的文字訊息（可留空）")]
//    public string message;

//    [Tooltip("顯示的圖片（若有設定則優先顯示圖片）")]
//    public Sprite splashSprite;
//}
