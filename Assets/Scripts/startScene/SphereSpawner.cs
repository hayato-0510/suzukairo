using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField] private GameObject logoSpherePrefab; // InspectorでPrefabをセット
    [SerializeField] private int numberOfSpheres = 5;   // 生成する球の数
    [SerializeField] private float spawnRadius = 2f;     // 初期配置のランダムな半径

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
}