using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class controller : MonoBehaviour
{
    [Header("�L�[")]
    public KeyCode MoveFront = KeyCode.W;
    public KeyCode RotateRight = KeyCode.D;
    public KeyCode MoveBack = KeyCode.S;
    public KeyCode RotateLeft = KeyCode.A;

    [Header("�X�s�[�h")]
    public float moveSpeed = 0.5f;
    public float rotateSpeed = 90f;
    public float conducutorMoveSpeed = 2f;
    public float nonconductorMoveSpeed = 0.5f;
    public float moveDecay = 3f;
    private Vector2 moveVelocity;

    [Header("�d��")]
    public float startAmpere = 0.1f;
    public float ampere=0.1f;

    [Header("�d��")]
    public float voltage=5.0f;
    public TextMeshProUGUI voltageText;

    [Header("���M�X�s�[�h")]
    public float radiationSpeed=1.0f;

    private Rigidbody2D rb;
    private Vector3 dir;
    public bool isOnConductor=false;
    private Vector3 startScale;

    [HideInInspector]
    public bool canMove=false;

    [Header("Boost")]
    public float boostSpeed = 100f;
    public float boostDecay = 3f;
    private Vector2 boostVelocity;
    public KeyCode BoostKey = KeyCode.Space;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ampere = startAmpere;
        startScale = transform.localScale;
    }

    private void Update()
    {
        voltageText.text = voltage.ToString("f"+1)+"V";
        if (Input.GetKeyDown(BoostKey))
        {
            if (voltage >= 1)
            {
                boostVelocity += (Vector2)transform.up * boostSpeed;
                voltage--;
            }
            else if(voltage>0)
            {
                boostVelocity += (Vector2)transform.up * boostSpeed * voltage;
                voltage = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        transform.localScale = ampere / startAmpere * startScale;
        if (!canMove) return;

        float forward = 0f;

        if (Input.GetKey(MoveFront))
        {
            forward = 1f;
            if (!isOnConductor) ampere /= 1f + Time.deltaTime * radiationSpeed;
        }
        if (Input.GetKey(MoveBack))
        {
            forward = -1f;
            if (!isOnConductor) ampere /= 1f + Time.deltaTime * radiationSpeed;
        }

        if (forward != 0)
        {
            Vector2 target = transform.up * forward * moveSpeed * (ampere * 10);
            moveVelocity = target;
        }
        else
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, Time.fixedDeltaTime * moveDecay);
        }

        boostVelocity = Vector2.Lerp(boostVelocity, Vector2.zero, Time.fixedDeltaTime * boostDecay);

        rb.linearVelocity = moveVelocity + boostVelocity;


        // ��]
        float turn = 0f;
        if (Input.GetKey(RotateRight)) turn = -1f;
        if (Input.GetKey(RotateLeft)) turn = 1f;

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
