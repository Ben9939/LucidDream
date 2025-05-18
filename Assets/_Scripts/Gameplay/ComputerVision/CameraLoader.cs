using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// CameraLoader は、Python スクリプト側と通信してカメラ起動状態を取得するためのローダーです。
/// </summary>
public class CameraLoader : MonoBehaviour, IScriptLoader
{
    // UDP クライアント（"CameraStarted" メッセージ受信用）
    private UdpClient startClient;
    private int selectedStartPort;

    // UDP クライアント（データ受信用；後続シーンで利用するためクローズしない）
    private UdpClient dataClient;
    private int selectedDataPort;

    private bool shouldContinueReceiving = true;
    public event Action OnLoaded;

    // メインスレッドで実行するアクションを保持するキュー
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    /// <summary>
    /// UpdateHandle はメインスレッドでキュー内のアクションを順次実行します。
    /// </summary>
    public void UpdateHandle()
    {
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// ロード処理を開始します
    /// </summary>
    public void StartLoading()
    {
        InitializeLoader();
    }

    /// <summary>
    /// ロード処理を停止します（startClient を閉じます）
    /// </summary>
    public void StopLoading()
    {
        shouldContinueReceiving = false;
        if (startClient != null)
        {
            try { startClient.Close(); } catch (Exception e) { Debug.LogWarning(e.Message); }
            startClient = null;
        }
    }

    /// <summary>
    /// ネットワーククライアントの初期化と Python スクリプト起動を行います
    /// </summary>
    private void InitializeLoader()
    {
        shouldContinueReceiving = true;

        // 1. startClient の生成（動的ポート割り当て）
        try
        {
            startClient = new UdpClient(0);
            selectedStartPort = ((IPEndPoint)startClient.Client.LocalEndPoint).Port;
            Debug.Log($"CameraLoader startClient bound to port {selectedStartPort}.");
            NetworkConfig.Mode = "hand";
            NetworkConfig.StartPort = selectedStartPort;
        }
        catch (Exception e)
        {
            Debug.LogError($"CameraLoader failed to bind startClient: {e.Message}");
            return;
        }

        // 2. dataClient の生成（動的ポート割り当て、後続シーンで利用）
        try
        {
            dataClient = new UdpClient(0);
            selectedDataPort = ((IPEndPoint)dataClient.Client.LocalEndPoint).Port;
            Debug.Log($"CameraLoader dataClient bound to port {selectedDataPort}.");
            NetworkConfig.DataPort = selectedDataPort;
            NetworkConfig.DataClient = dataClient;
        }
        catch (Exception e)
        {
            Debug.LogError($"CameraLoader failed to bind dataClient: {e.Message}");
            return;
        }

        // 3. "CameraStarted" メッセージ受信開始
        startClient.BeginReceive(ReceiveCallback, null);

        // 4. Python スクリプト（UnifiedCamera.exe）を起動し、パラメータを渡す
        string pythonScriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, "UnifiedCamera.dist", "UnifiedCamera.exe");
        string arguments = $"--start_port {selectedStartPort} --data_port {selectedDataPort} --mode hand";
        if (PythonProcessManager.Instance != null)
        {
            PythonProcessManager.Instance.StartPythonScript(pythonScriptPath, arguments);
        }
        else
        {
            EnqueueToMainThread(() => Debug.LogError("PythonProcessManager instance not found."));
        }
    }

    /// <summary>
    /// 指定のアクションをメインスレッドで実行するためキューに追加します
    /// </summary>
    private void EnqueueToMainThread(Action action)
    {
        mainThreadActions.Enqueue(action);
    }

    /// <summary>
    /// UDP 受信コールバック。受信したメッセージに応じた処理を行います
    /// </summary>
    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            if (!shouldContinueReceiving) return;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = startClient.EndReceive(result, ref remoteEP);
            string message = Encoding.UTF8.GetString(data);
            EnqueueToMainThread(() => Debug.Log("CameraLoader received: " + message));

            if (message == "CameraStarted")
            {
                EnqueueToMainThread(() => Debug.Log("CameraLoader: CameraStarted received."));
                shouldContinueReceiving = false;
                try { startClient.Close(); } catch (Exception e) { Debug.LogWarning(e.Message); }
                startClient = null;
                EnqueueToMainThread(() => OnLoaded?.Invoke());
            }
            else
            {
                startClient.BeginReceive(ReceiveCallback, null);
            }
        }
        catch (Exception e)
        {
            EnqueueToMainThread(() => Debug.LogError("CameraLoader ReceiveCallback error: " + e.Message));
        }
    }
}
