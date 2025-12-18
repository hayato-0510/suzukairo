using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System.Linq; // 追加：ポート一覧チェック用

public class SerialHandler : MonoBehaviour
{
    public string portName = "COM2";
    public int baudRate = 115200;
    public event Action<string> OnDataReceived;
 
    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;
    private bool isConnected_ = false; // 追加：接続に成功したかどうかのフラグ
 
    private string message_;
    private bool isNewMessageReceived_ = false;
 
    void Start()
    {
        Open();
    }
 
    void Update()
    {
        if (isNewMessageReceived_ && isConnected_)
        {
            OnDataReceived?.Invoke(message_);
            isNewMessageReceived_ = false; // 受信フラグをリセット
        }
    }

    void OnDestroy()
    {
        Close();
    }

    public void SendMessage(string message)
    {
        if (isConnected_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try {
                serialPort_.WriteLine(message);
            } catch (System.Exception e) {
                Debug.LogWarning($"Send Failed: {e.Message}");
            }
        }
    }
 
    private void Open()
    {
        // 1. 利用可能なポートをチェック
        string[] availablePorts = SerialPort.GetPortNames();
        if (!availablePorts.Contains(portName))
        {
            Debug.LogWarning($"[Serial] Port {portName} not found. Running in NO-CONTROLLER mode.");
            isConnected_ = false;
            return; // 接続せず終了（例外は出さない）
        }

        try 
        {
            serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
            serialPort_.Open();
            serialPort_.ReadTimeout = 1000; // 読み込みタイムアウトを設定
            
            isConnected_ = true;
            isRunning_ = true;
            thread_ = new Thread(Read);
            thread_.Start();
            Debug.Log($"[Serial] Successfully connected to {portName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Serial] Failed to open {portName}: {e.Message}");
            isConnected_ = false;
        }
    }
 
    private void Read()
    {
        while (isRunning_ && isConnected_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                message_ = serialPort_.ReadLine();
                isNewMessageReceived_ = true;
            }
            catch (System.TimeoutException) { /* タイムアウトは無視 */ }
            catch (System.Exception e)
            {
                if (isRunning_) Debug.LogWarning($"[Serial] Read Error: {e.Message}");
            }
        }
    }
 
    private void Close()
    {
        isRunning_ = false;
        isConnected_ = false;
 
        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }
 
        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    // 外部から接続状態を確認できるようにプロパティを追加
    public bool isConnected => isConnected_;
}