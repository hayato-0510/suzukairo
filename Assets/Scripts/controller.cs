using NUnit.Framework;
using System.Collections.Generic;
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
    public float startAmpere = 0.1f;
    public float ampere=0.1f;
    [Header("放熱スピード")]
    public float radiationSpeed=1.0f;

    private Rigidbody2D rb;
    private Vector3 dir;
    private bool isOnConductor=false;
    private Vector3 startScale;

    [HideInInspector]
    public bool canMove=false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ampere = startAmpere;
        startScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        transform.localScale = ampere/startAmpere*startScale;
        if (!canMove) return;
        float forward = 0f;
        if (Input.GetKey(MoveFront))
        {
            forward = 1f; //前に移動
            if(!isOnConductor)ampere /= 1f + Time.deltaTime * radiationSpeed;
        }
        if (Input.GetKey(MoveBack)) 
        {
            forward = -1f; //後ろに移動
            if(!isOnConductor)ampere /= 1f + Time.deltaTime * radiationSpeed;
        }

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
            isOnConductor = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "conductor")
        {
            moveSpeed = conducutorMoveSpeed;
            isOnConductor = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "conductor")
        {
            moveSpeed = conducutorMoveSpeed;
            isOnConductor = true;
        }
    }
}
