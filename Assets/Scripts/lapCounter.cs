using UnityEngine;
using System.Collections.Generic;

public class LapCounter : MonoBehaviour
{
    public CircuitLoader loader;               // Loader を参照
    private List<int> checkpointOrder;  // Loader の checkpoint ID の順序
    private int nextIndex = 0;          // 次に通るべきチェックポイントのインデックス
    public int lap = 0;                 // 現在のラップ数

    private void Start()
    {
        // Loader からチェックポイントの順序を取得
        checkpointOrder = loader.checkPointIds;
        nextIndex = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ---------- Checkpoint ----------
        if (other.CompareTag("checkpoint"))
        {
            Debug.Log("A");
            componentData cp = other.GetComponent<componentData>();
            if (cp == null) return;

            int cpId = cp.id;

            // 順番通り？
            if (cpId == checkpointOrder[nextIndex])
            {
                nextIndex++;

                // 最後のチェックポイントまで行った？
                if (nextIndex >= checkpointOrder.Count)
                {
                    nextIndex = 0; // ゴールに行けばラップ加算できる状態
                }
            }
            return;
        }

        // ---------- Goal ----------
        if (other.CompareTag("goal"))
        {
            // 全チェックポイントを通り終えた状態でのみラップ加算
            if (nextIndex == 0)
            {
                lap++;
                Debug.Log("Lap: " + lap);
            }
        }
    }
}
