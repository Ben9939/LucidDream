using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UIManager クラスは、シーン内の対話 UI コンテナの参照を管理します。
/// シーン切り替え時に DialogUIContainer タグを持つオブジェクトを自動的に検索し、参照を更新します。
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 現在のシーン内の対話UIコンテナの参照
    // （対話UIが必要なシーンでは、対象オブジェクトの Tag を "DialogUIContainer" に設定してください）
    private GameObject dialogUIContainer;
    public GameObject DialogUIContainer => dialogUIContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // シーンロード後にコンテナの参照を更新する
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンのロードが完了した後、"DialogUIContainer" タグを持つオブジェクトを自動的に検索し、参照を更新する
    /// </summary>
    /// <param name="scene">ロードされたシーン</param>
    /// <param name="mode">シーンのロードモード</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dialogUIContainer = GameObject.FindWithTag("DialogUIContainer");
        if (dialogUIContainer == null)
        {
            Debug.LogWarning($"Dialog UI container with tag 'DialogUIContainer' not found in scene {scene.name}!");
        }
        else
        {
            Debug.Log($"Bound dialogUIContainer of scene {scene.name}: {dialogUIContainer.name}");
        }
    }
}
