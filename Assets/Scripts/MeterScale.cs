using UnityEngine;
using UnityEngine.UI;

public class MeterScale : MonoBehaviour
{
    [Header("目盛り設定")]
    public int scaleCount = 11;           // 目盛りの数（0を含む）
    public float minAngle = 225f;         // 最小角度
    public float maxAngle = 315f;         // 最大角度
    public float scaleLength = 20f;       // 目盛り線の長さ
    public float scaleThickness = 2f;     // 目盛り線の太さ
    public Color scaleColor = Color.white;// 目盛り線の色
    public float radius = 100f;           // メーターの半径

    [Header("主目盛り設定（5本おきなど）")]
    public bool useMainScale = true;
    public int mainScaleInterval = 5;     // 何本おきに太くするか
    public float mainScaleLength = 30f;   // 主目盛りの長さ
    public float mainScaleThickness = 3f; // 主目盛りの太さ

    void Start()
    {
        GenerateScales();
    }

    void GenerateScales()
    {
        for (int i = 0; i < scaleCount; i++)
        {
            // 角度を計算
            float t = (float)i / (scaleCount - 1);
            float angle = Mathf.Lerp(minAngle, maxAngle, t);

            // 主目盛りかどうか判定
            bool isMainScale = useMainScale && (i % mainScaleInterval == 0);
            float length = isMainScale ? mainScaleLength : scaleLength;
            float thickness = isMainScale ? mainScaleThickness : scaleThickness;

            // 目盛り線を作成
            CreateScaleLine(angle, length, thickness);
        }
    }

    void CreateScaleLine(float angle, float length, float thickness)
    {
        // 新しいGameObjectを作成
        GameObject scaleLine = new GameObject($"Scale_{angle}");
        scaleLine.transform.SetParent(transform, false);

        // RectTransformを追加
        RectTransform rectTransform = scaleLine.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(thickness, length);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0f); // 下端を基準点に

        // 位置を計算（メーターの中心から外側へ）
        float angleRad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        rectTransform.anchoredPosition = direction * (radius - length);

        // 回転を設定
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90f);

        // Imageコンポーネントを追加
        Image image = scaleLine.AddComponent<Image>();
        image.color = scaleColor;
    }

    // エディタでの確認用（Gizmosで表示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;

        for (int i = 0; i < scaleCount; i++)
        {
            float t = (float)i / (scaleCount - 1);
            float angle = Mathf.Lerp(minAngle, maxAngle, t);
            float angleRad = angle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
            Vector3 startPos = center + direction * (radius - scaleLength);
            Vector3 endPos = center + direction * radius;

            Gizmos.DrawLine(startPos, endPos);
        }
    }
}
