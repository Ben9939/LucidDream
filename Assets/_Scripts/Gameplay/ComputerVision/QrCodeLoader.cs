using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// QrCodeLoader は、QR コード用のデータを受信するためのローダーです。
/// </summary>
public class QrCodeLoader : MonoBehaviour, IScriptLoader
{
    // "CameraStarted" 受信用 UDP クライアント
    private UdpClient startClient;
    private int selectedStartPort;

    // QR データ受信用 UDP クライアント（後続シーンで利用するため閉じない）
    private UdpClient dataClient;
    private int selectedDataPort;

    private bool shouldContinueReceiving = true;
    public event Action OnLoaded;

    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    /// <summary>
    /// メインスレッドでキュー内のアクションを実行します
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
            Debug.Log($"QrCodeLoader startClient bound to port {selectedStartPort}.");
            NetworkConfig.Mode = "qr";
            NetworkConfig.StartPort = selectedStartPort;
        }
        catch (Exception e)
        {
            Debug.LogError($"QrCodeLoader failed to bind startClient: {e.Message}");
            return;
        }

        // 2. dataClient の生成（動的ポート割り当て、後続シーンで利用）
        try
        {
            dataClient = new UdpClient(0);
            selectedDataPort = ((IPEndPoint)dataClient.Client.LocalEndPoint).Port;
            Debug.Log($"QrCodeLoader dataClient bound to port {selectedDataPort}.");
            NetworkConfig.DataPort = selectedDataPort;
            NetworkConfig.DataClient = dataClient;
        }
        catch (Exception e)
        {
            Debug.LogError($"QrCodeLoader failed to bind dataClient: {e.Message}");
            return;
        }

        // 3. "CameraStarted" メッセージ受信開始
        startClient.BeginReceive(ReceiveCallback, null);

        // 4. Python スクリプト（UnifiedCamera.exe）を起動し、パラメータを渡す
        string pythonScriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, "UnifiedCamera.dist", "UnifiedCamera.exe");
        string arguments = $"--start_port {selectedStartPort} --data_port {selectedDataPort} --mode qr";
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
            EnqueueToMainThread(() => Debug.Log("QrCodeLoader received: " + message));
            if (message == "CameraStarted")
            {
                EnqueueToMainThread(() => Debug.Log("QrCodeLoader: CameraStarted received."));
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
            EnqueueToMainThread(() => Debug.LogError("QrCodeLoader ReceiveCallback error: " + e.Message));
        }
    }
}
