using System;
using UnityEngine;
using System.Collections;

public class SerialCommandRouter : MonoBehaviour
{
    [SerializeField] private SerialHandler serialHandler;
    
    public float turn;
    public float move;
    public float turbo;
    public int mapID = 0;

    public bool msw1 = false;
    public bool msw2 = false;

    private int right = 0;
    private int left = 0;

    void Start()
    {
        // SerializeFieldで設定されていない場合は検索
        if (serialHandler == null)
        {
            serialHandler = FindObjectOfType<SerialHandler>();
        }
        

        if (serialHandler != null)
        {
            // 常に購読して、接続後にも受け取れるようにする
            serialHandler.OnDataReceived += HandleSerialData;
            serialHandler.OnConnected += HandleConnected;

            if (serialHandler.isConnected)
            {
                HandleConnected();
            }
            else
            {
                // 未接続なら待つ（ログも控えめに）
                StartCoroutine(WaitForSerial());
            }
        }
        else
        {
            Debug.LogWarning("[SerialRouter] SerialHandler not found");
        }
    }

    private IEnumerator WaitForSerial()
    {
        yield return new WaitUntil(() => serialHandler != null && serialHandler.isConnected);
        HandleConnected();
    }

    private void HandleConnected()
    {
        Debug.Log("[SerialRouter] Serial connected and ready");
        // 必要なら疎通確認
        serialHandler?.SendMessage("health");
    }

    void OnDestroy()
    {
        if (serialHandler != null)
        {
            serialHandler.OnDataReceived -= HandleSerialData;
            serialHandler.OnConnected -= HandleConnected;
        }
    }

    private void HandleSerialData(string data)
    {
        // シリアルコマンド処理
        Debug.Log($"Received: {data}");
        string[] commands = data.Split(' ');
        switch (commands[0])
        {
            case "sw":
                if (commands[1] == "1")
                    move = 1f;
                else
                    move = 0f;

                left = commands[2] == "1" ? 1 : 0;
                right = commands[3] == "1" ? 1 : 0;
                turn = right - left;

                if (commands[4] == "1\r")
                    turbo = 1f;
                else
                    turbo = 0f;
                break;
            case "pad":

                if (commands[2] == "1")
                    move = 1f;
                else
                    move = 0f;

                left = commands[3] == "1" ? 1 : 0;
                right = commands[1] == "1" ? 1 : 0;
                turn = right - left;

                if (commands[4] == "1\r")
                    turbo = 1f;
                else
                    turbo = 0f;
                    
                break;
            case "map":
                mapID = int.Parse(commands[1]);
                break;
            case "msw":
                if (commands[1] == "1")
                    msw1 = true;
                else
                    msw1 = false;
                if (commands[2] == "1\r")
                    msw2 = true;
                else
                    msw2 = false;
                break;

            case "health":
                Debug.Log("[Serial] health check");
                serialHandler.SendMessage("ok");
                break;
            case "ok":
                Debug.Log("[Serial] helth ok");
                break;
            case "start":
                Debug.Log("[Serial] started");
                break;
        }
    }
}
