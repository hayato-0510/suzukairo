using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AICoachService : MonoBehaviour
{
    private const string API_URL = "https://ai-server-eight-silk.vercel.app/api/chat";

    [Serializable]
    private class ChatRequest
    {
        public List<Message> messages;
        public int max_tokens = 500;
        public float temperature = 0.7f;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    // OpenAI API Response Wrappers for JsonUtility
    [Serializable]
    private class StreamResponse
    {
        public string id;
        public List<StreamChoice> choices;
    }

    [Serializable]
    private class StreamChoice
    {
        public StreamDelta delta;
    }

    [Serializable]
    private class StreamDelta
    {
        public string content;
    }

    /// <summary>
    /// AIコーチにアドバイスを求める
    /// </summary>
    /// <param name="systemPrompt">AIの設定（役割など）</param>
    /// <param name="userPrompt">分析データ</param>
    /// <param name="onTokenReceived">1文字受信するごとのコールバック</param>
    /// <param name="onComplete">完了時のコールバック</param>
    public void GetAdvice(string systemPrompt, string userPrompt, Action<string> onTokenReceived, Action onComplete = null)
    {
        StartCoroutine(PostRequestCoroutine(systemPrompt, userPrompt, onTokenReceived, onComplete));
    }

    private IEnumerator PostRequestCoroutine(string systemPrompt, string userPrompt, Action<string> onTokenReceived, Action onComplete)
    {
        // リクエストボディの作成
        var reqData = new ChatRequest
        {
            messages = new List<Message>
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userPrompt }
            }
        };

        string jsonBody = JsonUtility.ToJson(reqData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // UnityWebRequestの設定
        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // リクエスト送信（非同期だが、DownloadHandlerBufferの進捗を監視する）
            var operation = request.SendWebRequest();

            int lastReadIndex = 0;

            while (!operation.isDone)
            {
                yield return null;
                ProcessStreamData(request.downloadHandler.text, ref lastReadIndex, onTokenReceived);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[AICoach] Error: {request.error}\nResponse: {request.downloadHandler.text}");
                onTokenReceived?.Invoke("\n(通信エラーが発生しました)");
            }
            else
            {
                // 残りのデータを処理
                ProcessStreamData(request.downloadHandler.text, ref lastReadIndex, onTokenReceived);
            }
        }

        onComplete?.Invoke();
    }

    private void ProcessStreamData(string fullText, ref int lastReadIndex, Action<string> onTokenReceived)
    {
        if (string.IsNullOrEmpty(fullText) || lastReadIndex >= fullText.Length) return;

        // 新しく受信した部分だけを切り出す
        string newText = fullText.Substring(lastReadIndex);
        
        // デバッグ：受信データを確認
        // Debug.Log($"[AICoach] Received chunk: {newText}");

        // 行ごとに処理
        string[] lines = newText.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            // SSEのフォーマット "data: {...}" を解析
            if (line.StartsWith("data: "))
            {
                string jsonPart = line.Substring(6).Trim();
                if (jsonPart == "[DONE]") continue;

                try
                {
                    // デバッグ：JSONパース試行
                    // Debug.Log($"[AICoach] Parsing JSON: {jsonPart}");

                    StreamResponse response = JsonUtility.FromJson<StreamResponse>(jsonPart);
                    if (response != null && response.choices != null && response.choices.Count > 0)
                    {
                        string content = response.choices[0].delta.content;
                        if (!string.IsNullOrEmpty(content))
                        {
                            onTokenReceived?.Invoke(content);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AICoach] Parse Error: {e.Message} in line: {line}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                // data: で始まらない行（エラーメッセージなどの可能性）
                Debug.LogWarning($"[AICoach] Unexpected line format: {line}");
                
                // エラーがJSONで返ってきている場合の簡易チェック
                if (line.Trim().StartsWith("{") && line.Contains("error"))
                {
                     onTokenReceived?.Invoke($"Error: {line}");
                }
            }
        }

        // 読み込み位置を更新
        lastReadIndex = fullText.Length;
    }
}
