using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.VisualBasic;

[DefaultExecutionOrder(-1000)]
public class SerialHandler : MonoBehaviour
{
    public int baudRate = 115200;
    public event Action<string> OnDataReceived;
    public int timeoutMs = 250;
    // 接続に使用するポート名（自動検出で上書きされる）
    public string portName = null;
    public string probeMessage = "health";
    public string expectedResponsePrefix = "ok";

    public bool isConnected = false; // 追加：接続に成功したかどうかのフラグ
    public bool isStarted = false; //開始できるかどうか

    public bool verboseLog = true;
    private SerialPort serialPort;
    private Thread thread;
    private bool isPortOpen = false;

    private string message = null;
    private bool isNewMessageReceived = false;

    [SerializeField] private List<string> DeviceList = new List<string>();
    [SerializeField] string[] activePorts = null;

    private static SerialHandler instance;
    public static SerialHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SerialHandler>();
                if (instance == null)
                {
                    Debug.LogError("MySerialHandler instance not found in the scene. Please ensure MySerialHandler is attached to a GameObject in the scene.");
                }
            }
            return instance;
        }
    }

    async void Awake()
    {   
        if (instance && this != instance)
        {
            Debug.Log("Destroyed");
            Destroy(this.gameObject);
            return; // この行を追加して、以下のコードが実行されないようにする

        }

        instance = this;
        // Scene遷移で破棄されなようにする。      
        DontDestroyOnLoad(this);

        Thread.Sleep(2000);
        
        if (!isPortOpen)
        {
            portName = await SearchPort();
        }
        
        if (portName != null)
        {
            OpenToRead();// Deviceのポートを開く
            
            thread = new Thread(Read);
            thread.Start();
            Thread.Sleep(100);// 100ms待つ
        }
        else
        {
            isStarted = true;
        }
    }

    //自動検索
    private async Task<string> SearchPort()
    {
        Debug.Log("SearchPort Start");
        activePorts = SerialPort.GetPortNames()
        .Where(p =>
            p.IndexOf("COM", StringComparison.OrdinalIgnoreCase) >= 0 ||
            p.IndexOf("usb", StringComparison.OrdinalIgnoreCase) >= 0)
        .ToArray();

        foreach (string port in activePorts)
        {
            Debug.Log(port + " is active");

        }

        for (int i = 0; i < activePorts.Length; i++)
        {
            portName = activePorts[i]; // activePortの名前をportNameに代入する
            message = null; // メッセージをクリアする
            try
            {
                OpenToSearch();
                Thread.Sleep(2000);
                SendMessage(probeMessage);
                await ReadAsync();// 非同期にデータを読み取る

                Close();

                if (message != null && message.Contains(expectedResponsePrefix))
                {
                    Debug.Log(portName + " is Device!");
                    DeviceList.Add(portName);// Deviceのリストに追加
                    Debug.Log(portName);

                    return portName;// Deviceのポート名を返す
                }
                else
                {
                    Debug.Log(portName + " is not Device");
                    Close();
                    Thread.Sleep(1000);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                Close();
            }

        }
        Debug.Log("Device is not found in active ports");
        return null;
    }

    private void OpenToSearch()
    {
        serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = 500//タイムアウトの設定
        };
        serialPort.DtrEnable = false;
        serialPort.RtsEnable = false;
        serialPort.Open();//ポートを開く
        isPortOpen = true; // ポートが開かれたことを示す
    }

    //非同期でデータを読み取る 初期接続用
    public async Task<string> ReadAsync()
    {
        message = null;
        await Task.Run(() =>
        {
            while (true)
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    try
                    {
                        message = serialPort.ReadLine();
                        Debug.Log(message + " is received");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("2:" + e.Message);
                        break;
                    }

                    if(message.Contains(expectedResponsePrefix))
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                    
                }
                else
                {
                    break;
                }
            }
        });
        return message;
    }
    
    private void OpenToRead()
    {
        serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = 1000//タイムアウトの設定
        };
        serialPort.Open();//ポートを開く
        isPortOpen = true; // ポートが開かれたことを示す
        isConnected = true;
        isStarted = true;
    }

    public void SendMessage(string message)
    {
        if (isPortOpen && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                Debug.Log($"Send: {message}");
                serialPort.WriteLine(message);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Send Failed: {e.Message}");
            }
        }
    }

    private void Read()
    {
        Debug.Log("Read Start");
        while (isPortOpen && isConnected && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                message = serialPort.ReadLine();
                isNewMessageReceived = true;
            }
            catch (System.TimeoutException) { /* タイムアウトは無視 */ }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Serial] Read Error: {e.Message}");
            }
        }
    }

    private void Close()
    {
        isPortOpen = false;
        isConnected = false;

        if (thread != null && thread.IsAlive)
        {
            thread.Join();
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }

    void Update()
    {
        if (isNewMessageReceived && isConnected)
        {
            OnDataReceived?.Invoke(message);
            isNewMessageReceived = false; // 受信フラグをリセット
        }
    }

    void OnDestroy()
    {
        Close();
    }
    
}
