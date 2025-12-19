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

            if(!serialHandler.isStarted)
                StartCoroutine(WaitForSerial());
        }
        else
        {
            Debug.LogWarning("[SerialRouter] SerialHandler not found");
        }
    }

    private IEnumerator WaitForSerial()
    {
        yield return new WaitUntil(() => serialHandler != null && serialHandler.isStarted);
    }


    void OnDestroy()
    {
        if (serialHandler != null)
        {
            serialHandler.OnDataReceived -= HandleSerialData;
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

                 if(commands[2] == "1" && commands[3] == "1")
                    turn = 0;
                else if(commands[2] == "1" && commands[3] == "0")
                    turn = -1;
                else if(commands[2] == "0" && commands[3] == "1")
                    turn = 1;
                else
                    turn = 0;

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

                if(commands[3] == "1" && commands[1] == "1")
                    turn = 0;
                else if(commands[3] == "1" && commands[1] == "0")
                    turn = 1;
                else if(commands[3] == "0" && commands[1] == "1")
                    turn = -1;
                else
                    turn = 0;

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
