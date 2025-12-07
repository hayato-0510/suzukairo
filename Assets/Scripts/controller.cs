using UnityEngine;

public class controller : MonoBehaviour
{
    [Header("キー")]
    public KeyCode MoveFront = KeyCode.W;
    public KeyCode RotateRight = KeyCode.D;
    public KeyCode MoveBack = KeyCode.S;
    public KeyCode RotateLeft = KeyCode.A;

    [Header("スピード")]
    public float moveSpeed = 0.5f;
    public float rotateSpeed = 90f;
    public float conducutorMoveSpeed = 2f;
    public float nonconductorMoveSpeed = 0.5f;

    [Header("電流")]
    public float ampere=0.1f;

    private Rigidbody2D rb;
    private Vector3 dir;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float forward = 0f;
        if (Input.GetKey(MoveFront)) forward = 1f; //前に移動
        if (Input.GetKey(MoveBack)) forward = -1f; //後ろに移動

        Vector2 move = transform.up * forward * moveSpeed * (ampere*10);

        rb.linearVelocity = move;

        float turn = 0f;
        if (Input.GetKey(RotateRight)) turn = -1f; // 右回転
        if (Input.GetKey(RotateLeft)) turn = 1f;  // 左回転

        rb.MoveRotation(rb.rotation + turn * rotateSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "conductor")
        {
            moveSpeed = nonconductorMoveSpeed;
            Debug.Log(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "conductor")
        {
            moveSpeed = conducutorMoveSpeed;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "conductor")
        {
            moveSpeed = conducutorMoveSpeed;
        }
    }
}
