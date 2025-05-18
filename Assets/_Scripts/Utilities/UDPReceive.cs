using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// UDPReceive クラスは、UDP 通信でデータを受信し、処理を行います。
/// 受信したデータは、指定されたポートから取得され、必要に応じてログ出力します。
/// </summary>
public class UDPReceive : MonoBehaviour
{
    private Thread receiveThread;
    private UdpClient client;
    private int selectedPort;
    public bool startRecieving = true;
    public bool printToConsole = false;
    public string data;

    /// <summary>
    /// 初期化処理。既存のクライアントがあれば再利用し、なければ動的にバインドします。
    /// </summary>
    public void Start()
    {
        if (NetworkConfig.DataClient != null)
        {
            client = NetworkConfig.DataClient;
            selectedPort = NetworkConfig.DataPort;
            Debug.Log($"UDPReceive reusing saved UDP client on port {selectedPort} from NetworkConfig.");
        }
        else
        {
            try
            {
                client = new UdpClient(0);
                selectedPort = ((IPEndPoint)client.Client.LocalEndPoint).Port;
                Debug.Log($"UDPReceive bound to dynamic port {selectedPort}.");
            }
            catch (Exception e)
            {
                Debug.LogError($"UDPReceive failed to bind: {e.Message}");
                return;
            }
        }

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    /// <summary>
    /// UDP でデータを受信する処理を実行します。
    /// </summary>
    private void ReceiveData()
    {
        while (startRecieving)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                data = Encoding.UTF8.GetString(dataByte);
                if (printToConsole)
                {
                    Debug.Log("UDPReceive received: " + data);
                }
            }
            catch (Exception err)
            {
                if (startRecieving)
                {
                    Debug.LogWarning("UDPReceive error: " + err.ToString());
                }
            }
        }
    }

    /// <summary>
    /// リソースの解放処理。受信スレッドの終了と UDP クライアントのクローズを行います。
    /// </summary>
    public void ReleaseResources()
    {
        startRecieving = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            try
            {
                if (!receiveThread.Join(500))
                {
                    receiveThread.Interrupt();
                    Debug.LogWarning("UDPReceive thread forced to interrupt.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error while waiting for UDPReceive thread to join: " + e.Message);
            }
        }
        if (client != null)
        {
            try { client.Close(); } catch (Exception e) { Debug.LogWarning("Error closing UDPReceive client: " + e.Message); }
        }
        Debug.Log("UDPReceive resources have been released.");
    }
}
