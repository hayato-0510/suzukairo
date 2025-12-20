using UnityEngine;

public class CheckpointArrow : MonoBehaviour
{
    public LapCounter lapCounter;
    public Transform electron;
    public float rotateSpeed = 720f; // 回転補間（deg/sec）

    private void Update()
    {
        transform.position = electron.position;

        Transform target = lapCounter.getNextCheckpoint();

        Vector2 dir = (target.position - transform.position).normalized;

        // 2D用角度（↑を正面にしたい場合）
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        // スムーズに回転
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}