using UnityEngine;
using UnityEngine.UI;

public class MeterGrip : MonoBehaviour
{
    [Header("グリップ設定")]
    public int gripCount = 24;              // グリップの数
    public float radius = 120f;             // グリップを配置する円の半径
    public Vector2 gripSize = new Vector2(8f, 15f); // グリップのサイズ（幅x高さ）
    public Color gripColor = Color.white;   // グリップの色
    public bool useCircleShape = false;     // 円形グリップを使うか

    [Header("グリップ範囲")]
    public bool limitAngleRange = false;    // 角度範囲を制限するか
    public float startAngle = 0f;           // グリップの開始角度
    public float endAngle = 360f;           // グリップの終了角度

    void Start()
    {
        GenerateGrips();
    }

    void GenerateGrips()
    {
        float angleRange = limitAngleRange ? (endAngle - startAngle) : 360f;
        float angleStart = limitAngleRange ? startAngle : 0f;

        for (int i = 0; i < gripCount; i++)
        {
            // 角度を計算
            float angle = angleStart + (angleRange * i / gripCount);

            // グリップを作成
            CreateGrip(angle);
        }
    }

    void CreateGrip(float angle)
    {
        // 新しいGameObjectを作成
        GameObject grip = new GameObject($"Grip_{angle}");
        grip.transform.SetParent(transform, false);

        // RectTransformを追加
        RectTransform rectTransform = grip.AddComponent<RectTransform>();
        
        if (useCircleShape)
        {
            // 円形の場合は正方形
            float size = Mathf.Max(gripSize.x, gripSize.y);
            rectTransform.sizeDelta = new Vector2(size, size);
        }
        else
        {
            rectTransform.sizeDelta = gripSize;
        }
        
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 位置を計算（円周上に配置）
        float angleRad = angle * Mathf.Deg2Rad;
        Vector2 position = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        rectTransform.anchoredPosition = position;

        // 回転を設定（円の中心方向を向く）
        if (!useCircleShape)
        {
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        // Imageコンポーネントを追加
        Image image = grip.AddComponent<Image>();
        image.color = gripColor;
        
        // 円形の場合は、Imageのタイプを変更
        if (useCircleShape)
        {
            image.type = Image.Type.Simple;
            // 円形スプライトを使う場合はここでspriteを設定
            // image.sprite = yourCircleSprite;
        }
    }

    // エディタでの確認用
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;

        float angleRange = limitAngleRange ? (endAngle - startAngle) : 360f;
        float angleStart = limitAngleRange ? startAngle : 0f;

        for (int i = 0; i < gripCount; i++)
        {
            float angle = angleStart + (angleRange * i / gripCount);
            float angleRad = angle * Mathf.Deg2Rad;

            Vector3 position = center + new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * radius;
            Gizmos.DrawWireSphere(position, 5f);
        }

        // 円を描画
        Gizmos.color = Color.yellow;
        int segments = 64;
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float a = angleStart + angleRange * t;
            float rad = a * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
            
            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }
            prevPoint = point;
        }
    }
}
