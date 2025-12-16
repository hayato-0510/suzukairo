using UnityEngine;
using System.Collections.Generic;

public class LogoSphere : MonoBehaviour
{
    [Header("物理挙動設定")]
    [SerializeField] private float centralForce = 0.5f;        // 中心への引力 (弱め)
    [SerializeField] private float repulsionForce = 50f;      // 相互間の反発力 (強め)
    [SerializeField] private float maxRepulsionDistance = 3f; // 反発力が働く最大距離

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbodyがアタッチされていません。LogoSphereスクリプトを無効化します。");
            enabled = false;
        }
        
        // 重力の影響を受けないように再確認
        if (rb != null)
        {
             rb.useGravity = false;
        }
    }

    // 物理演算の適用はFixedUpdateで行う
    void FixedUpdate()
    {
        // 1. 中心への引力 (画面中央への集約)
        ApplyCentralAttraction();

        // 2. 他の球体との反発
        ApplyRepulsionFromOthers();
    }

    private void ApplyCentralAttraction()
    {
        // 画面中央 (0, 0, 0) を目指す
        Vector3 center = Vector3.zero;
        Vector3 directionToCenter = center - transform.position;

        // 引力を適用 (ForceMode.Acceleration は質量を無視して力を加える)
        rb.AddForce(directionToCenter.normalized * centralForce, ForceMode.Acceleration);
    }

    private void ApplyRepulsionFromOthers()
    {
        // Scene内のすべてのLogoSphereを取得 (パフォーマンスはやや低下しますが、シンプルです)
        LogoSphere[] allSpheres = FindObjectsOfType<LogoSphere>();
        
        foreach (LogoSphere other in allSpheres)
        {
            if (other == this) continue; // 自分自身はスキップ

            Vector3 difference = transform.position - other.transform.position;
            float distance = difference.magnitude;

            if (distance < maxRepulsionDistance && distance > 0.01f)
            {
                // 距離の二乗に反比例する反発力を適用 (電子のような挙動)
                // 距離が近いほど力が急激に強くなる
                float repulsionStrength = repulsionForce / (distance * distance);
                
                // 力の方向は、相手から遠ざかる方向
                rb.AddForce(difference.normalized * repulsionStrength, ForceMode.Acceleration);
            }
        }
    }
}