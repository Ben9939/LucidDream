using UnityEngine;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceEffect : MonoBehaviour
{
    //private AudioSource audioSource;

    ////[SerializeField] private float maxVolume = 0.5f;    // 最大音量
    ////[SerializeField] private LayerMask obstacleLayer; // 障碍物层
    ////private List<Vector2> path;
    //private float maxDistance; // 最大有效距离
    //private Vector2 audioSourceStarPosition = Vector2.zero;
    //private Vector2 audioSourceEndPosition = Vector2.zero;
    //private Transform playerTransform;

    //private void Start()
    //{
        
    //    audioSource = GetComponent<AudioSource>();
    //    playerTransform = PlayerController.Instance.transform; // 获取玩家位置
    //    //path = MazeSolver.Instance.GetPath();
    //    //audioSourceStarPosition = path[0];
    //    //audioSourceEndPosition = path[path.Count - 1];
    //    maxDistance = Vector2.Distance(audioSourceStarPosition, audioSourceEndPosition);
    //    UpdataAudioSourcePosition(audioSourceEndPosition);
    //}
    //private void UpdataAudioSourcePosition(Vector2 position)
    //{
    //    this.transform.position = position;
    //}
    //private void Update()
    //{
        
    //    if (playerTransform == null) return;

    //    // 获取玩家和音源的位置
    //    Vector3 playerPosition = playerTransform.position;
    //    Vector3 sourcePosition = transform.position;

    //    //// 1. 计算玩家到路径的最短距离
    //    //float minDistanceToPath = float.MaxValue;
    //    //foreach (Vector2 pathPoint in path)
    //    //{
    //    //    float distance = Vector2.Distance(playerPosition, pathPoint);
    //    //    if (distance < minDistanceToPath)
    //    //    {
    //    //        minDistanceToPath = distance;
    //    //    }
    //    //}

    //    //// 根据偏离主干道的距离调整音量（线性衰减）
    //    //float pathInfluence = Mathf.Clamp01(1f - minDistanceToPath / maxDistance);

    //    // 2. 根据距离终点调整音量（线性递增）
    //    float distanceToEnd = Vector2.Distance(playerPosition, audioSourceEndPosition);
    //    float endProximityInfluence = Mathf.Clamp01(1f - distanceToEnd / maxDistance);

    //    // 3. 检查障碍物的遮挡
    //    float obstacleFactor = 1f;
    //    RaycastHit2D hit = Physics2D.Raycast(sourcePosition, playerPosition - sourcePosition, Vector2.Distance(playerPosition, sourcePosition), obstacleLayer);
    //    if (hit.collider != null)
    //    {
    //        obstacleFactor = 0.5f; // 若有遮挡，音量减半
    //    }

    //    // 综合调整音量
    //    audioSource.volume = maxVolume * pathInfluence * endProximityInfluence * obstacleFactor;
    
    //}



}
