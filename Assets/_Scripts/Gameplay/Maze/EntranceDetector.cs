using UnityEngine;
using UnityEngine.Tilemaps;

public class EntranceDetector : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private GameObject targetObject;  // 表示/非表示を制御する対象オブジェクト
    [SerializeField] private Tilemap interiorTilemap;    // 室内領域を定義する Tilemap
    [SerializeField] private Transform playerTransform;  // プレイヤーの Transform

    // 前回の室内状態を保持する変数
    private bool previousIsInside = false;

    private void Update()
    {
        // プレイヤーの位置を Tilemap のセル座標に変換する
        Vector3Int cellPos = interiorTilemap.WorldToCell(playerTransform.position);
        // 該当セルに Tile が存在するかチェック（存在すれば室内と判断）
        bool isInside = interiorTilemap.HasTile(cellPos);

        // プレイヤーが室内なら対象オブジェクトを非表示、室外なら表示する
        targetObject.SetActive(!isInside);

        // もし前回は室内で、今回室外に移動した場合のみSEを再生する
        if (previousIsInside && !isInside)
        {
            AudioManager.Instance.PlaySE(SESoundData.SE.OpenDoor);
        }

        // 現在の状態を前回の状態として更新
        previousIsInside = isInside;
    }
}
