using UnityEngine;

public class SerialCommandRouter : MonoBehaviour
{
    [SerializeField] private SerialHandler serialHandler;
    public float turn;
    public float move;
    public float turbo;
    public int mapID = 0;

    void Start()
    {
        // SerializeFieldで設定されていない場合は検索
        if (serialHandler == null)
        {
            serialHandler = FindObjectOfType<SerialHandler>();
        }
        
        // シリアルハンドラーが存在し、接続されている場合のみイベント登録
        if (serialHandler != null && serialHandler.isConnected)
        {
            serialHandler.OnDataReceived += HandleSerialData;
            Debug.Log("[SerialRouter] Connected to serial handler");
        }
        else
        {
            Debug.Log("[SerialRouter] Running in keyboard-only mode");
        }
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
        //Debug.Log($"Received: {data}");
        string[] commands = data.Split(' ');
        switch (commands[0])
        {
            case "sw":
                if(commands[1] == "1")
                    move = 1f;
                else
                    move = 0f;

                if(commands[2] == "1")
                    turn = -1f;
                else 
                    turn = 0f;

                if(commands[3] == "1")
                    turn = 1f;
                else
                    turn = 0f;

                if(commands[4] == "1")
                    turbo = 1f;
                else
                    turbo = 0f;
                break;
            case "pad":
                if(commands[1] == "1")
                    turn = 1f;
                else
                    turn = 0f;

                if(commands[2] == "1")
                    move = 1f;
                else 
                    move = 0f;

                if(commands[3] == "1")
                    turn = -1f;
                else
                    turn = 0f;

                if(commands[4] == "1")
                    turbo = 1f;
                else
                    turbo = 0f;
                break;
            case "map":
                mapID = int.Parse(commands[1]);
                break;
            case "health":
                Debug.Log("[Serial] health check");
                serialHandler.SendMessage("ok");
                break;
            case "start":
                Debug.Log("[Serial] started");
                break;
        }
    }
}