using System.Net.Sockets;
using System.IO;
using UnityEngine;
using System.Diagnostics;
using System;

/// <summary>
/// ネットワーク設定に関する定数や共有データを管理する静的クラスです。
/// Mode: "hand" または "qr" の値をとります。
/// StartPort, DataPort: ネットワーク通信で使用するポート番号。
/// DataClient: UDP 通信に使用するクライアント。
/// </summary>
public static class NetworkConfig
{
    public static string Mode = "hand"; // or "qr"
    public static int StartPort = -1;
    public static int DataPort = -1;
    public static UdpClient DataClient = null;
}

/// <summary>
/// PythonProcessManager クラスは、打包後の Python 実行ファイルを起動・停止する処理を提供します。
/// シングルトンパターンを採用し、シーン間でインスタンスを共有します。
/// </summary>
public class PythonProcessManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static PythonProcessManager Instance { get; private set; }

    // 起動した Python プロセスを保持するプライベート変数
    private Process pythonProcess;

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// StreamingAssets フォルダ内の Python 実行ファイルを起動し、指定された引数を渡します。
    /// </summary>
    /// <param name="scriptRelativePath">StreamingAssets からの相対パス</param>
    /// <param name="arguments">起動時の引数</param>
    public void StartPythonScript(string scriptRelativePath, string arguments)
    {
        // StreamingAssets 内の Python 実行ファイルのフルパスを取得
        string pythonScriptPath = Path.Combine(Application.streamingAssetsPath, scriptRelativePath);
        if (!File.Exists(pythonScriptPath))
        {
            UnityEngine.Debug.LogError("Python executable not found at: " + pythonScriptPath);
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonScriptPath, // 実行ファイルを指定
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            pythonProcess = new Process { StartInfo = startInfo };
            pythonProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    UnityEngine.Debug.Log("[Python STDOUT] " + e.Data);
            };
            pythonProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    UnityEngine.Debug.LogError("[Python STDERR] " + e.Data);
            };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();
            UnityEngine.Debug.Log("Python script started with arguments: " + arguments);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to start Python script: " + e.Message);
        }
    }

    /// <summary>
    /// 実行中の Python プロセスを停止し、NetworkConfig の設定をリセットします。
    /// </summary>
    public void StopPythonScript()
    {
        if (pythonProcess != null)
        {
            try
            {
                if (!pythonProcess.HasExited)
                {
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("Error stopping Python process: " + e.Message);
            }
            finally
            {
                pythonProcess.Close();
                pythonProcess.Dispose();
                pythonProcess = null;
            }
        }

        // 必要に応じて NetworkConfig をリセット
        NetworkConfig.Mode = "hand";
        NetworkConfig.StartPort = -1;
        NetworkConfig.DataPort = -1;
        NetworkConfig.DataClient = null;

        UnityEngine.Debug.Log("Python process stopped and NetworkConfig has been reset.");
    }

    private void OnApplicationQuit()
    {
        StopPythonScript();
    }
}
