using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField] private GameObject logoSpherePrefab; // InspectorでPrefabをセット
    [SerializeField] private int numberOfSpheres = 5;   // 生成する球の数
    [SerializeField] private float spawnRadius = 2f;     // 初期配置のランダムな半径

    [Header("追加の球設定")]
    [SerializeField] private GameObject[] additionalSpherePrefabs; // コースごとの追加の球（配列）
    [SerializeField] private int[] additionalSphereCounts; // コースごとの生成個数（配列）
    [SerializeField] private float additionalSphereHeight = 5f; // 追加の球を降らせる高さ

    [Header("初期挙動設定")]
    [SerializeField] private float initialForceMagnitude = 1f; // 初期に与える力の強さ
    
    void Start()
    {
        if (logoSpherePrefab == null)
        {
            Debug.LogError("Logo Sphere Prefabが設定されていません。");
            return;
        }

        SpawnSpheres();
    }

    private void SpawnSpheres()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            // ランダムな初期位置を生成
            // Random.insideUnitCircleを使用して、Y=0の平面に配置する
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = new Vector3(randomCircle.x, 0, randomCircle.y);
            
            GameObject newSphere = Instantiate(logoSpherePrefab, randomPos, Quaternion.identity);
            newSphere.transform.SetParent(transform); // Hierarchyを整理

            // Rigidbodyを取得し、初期の力を与える
            Rigidbody rb = newSphere.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 球が散らばるように、ランダムな方向の力を一度だけ与える (Z軸方向の動きも許可)
                Vector3 randomDirection = Random.insideUnitSphere.normalized;
                rb.AddForce(randomDirection * initialForceMagnitude, ForceMode.Impulse);
                
                // ランダムな回転も与える
                rb.AddTorque(Random.insideUnitSphere * initialForceMagnitude * 0.5f, ForceMode.Impulse);
            }
        }
    }

    /// <summary>
    /// 既存の球を全て削除する
    /// </summary>
    public void ClearAllSpheres()
    {
        // このオブジェクトの子オブジェクトを全て削除
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("既存の球を全て削除しました。");
    }

    /// <summary>
    /// 最初の状態に戻す（初期の球を再生成）
    /// </summary>
    public void ResetToInitialSpheres()
    {
        ClearAllSpheres();
        SpawnSpheres();
        Debug.Log("初期の球を再生成しました。");
    }

    /// <summary>
    /// コース選択後に呼び出す。追加の球を指定された個数降らせる。
    /// </summary>
    /// <param name="courseIndex">選択されたコースのインデックス（0, 1, 2...）</param>
    public void SpawnAdditionalSphere(int courseIndex)
    {
        if (additionalSpherePrefabs == null || additionalSpherePrefabs.Length == 0)
        {
            Debug.LogWarning("Additional Sphere Prefabsが設定されていません。");
            return;
        }

        // コースインデックスが範囲外の場合は最初のprefabを使用
        if (courseIndex < 0 || courseIndex >= additionalSpherePrefabs.Length)
        {
            Debug.LogWarning($"コースインデックス {courseIndex} が範囲外です。最初のprefabを使用します。");
            courseIndex = 0;
        }

        GameObject prefabToSpawn = additionalSpherePrefabs[courseIndex];
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"コース {courseIndex} のprefabが設定されていません。");
            return;
        }

        // 生成個数を取得（配列が設定されていない場合はデフォルト1個）
        int count = 1;
        if (additionalSphereCounts != null && courseIndex < additionalSphereCounts.Length)
        {
            count = additionalSphereCounts[courseIndex];
        }

        // 指定された個数の球を生成
        for (int i = 0; i < count; i++)
        {
            // ランダムな水平位置、上空から降らせる
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(randomCircle.x, additionalSphereHeight, randomCircle.y);
            
            GameObject newSphere = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            newSphere.transform.SetParent(transform);

            // Rigidbodyがあれば重力で落下
            Rigidbody rb = newSphere.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 少しランダムな回転を与える
                rb.AddTorque(Random.insideUnitSphere * initialForceMagnitude * 0.3f, ForceMode.Impulse);
            }
        }
        
        Debug.Log($"コース {courseIndex} の追加の球を {count}個 生成しました。");
    }
}