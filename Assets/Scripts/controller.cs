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

    // シリアル入力用の変数を追加
    private SerialCommandRouter serialRouter;
    private float forward = 0f;
    private float turn = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ampere = startAmpere;
        startScale = transform.localScale;

        // SerialCommandRouter を取得
        serialRouter = FindObjectOfType<SerialCommandRouter>();
    }

    private void Update()
    {
        if (!canMove) return;

        // デフォルト値をリセット
        forward = 0f;
        turn = 0f;

        // キーボード入力を常に受け付ける
        HandleKeyboardInput();
        
        // シリアル入力がある場合は上書き
        if (serialRouter != null)
        {
            HandleSerialInput();
        }
        
        // 移動処理
        ApplyMovement();
    }

    private void HandleKeyboardInput()
    {
        // 前進/後退
        if (Input.GetKey(MoveFront)) forward = 1f;
        else if (Input.GetKey(MoveBack)) forward = -1f;

        // 回転
        if (Input.GetKey(RotateRight)) turn = -1f;
        else if (Input.GetKey(RotateLeft)) turn = 1f;

        // ブースト
        if (Input.GetKeyDown(BoostKey) && voltage >= 1)
        {
            boostVelocity += (Vector2)transform.up * boostSpeed;
            voltage--;
        }
    }

    private void HandleSerialInput()
    {
        // シリアルからの入力で上書き（値が0でない場合のみ）
        if (serialRouter.move != 0f)
            forward = serialRouter.move;
            
        if (serialRouter.turn != 0f)
            turn = serialRouter.turn;
            
        if (serialRouter.turbo > 0f && voltage >= 1)
        {
            boostVelocity += (Vector2)transform.up * boostSpeed;
            voltage--;
            serialRouter.turbo = 0f; // 1回のみ発動
        }
    }

    private void ApplyMovement()
    {
        // タイポを修正: startAmpare → startAmpere
        transform.localScale = ampere / startAmpere * startScale;
        if (!canMove) return;

        // 電流の減衰処理（導体外で移動時）
        if (forward != 0 && !isOnConductor)
        {
            ampere /= 1f + Time.deltaTime * radiationSpeed;
        }

        // 移動速度の計算
        if (forward != 0)
        {
            Vector2 target = (Vector2)transform.up * forward * moveSpeed * (ampere * 10);
            moveVelocity = target;
        }
        else
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, Time.fixedDeltaTime * moveDecay);
        }

        // ブーストの減衰
        boostVelocity = Vector2.Lerp(boostVelocity, Vector2.zero, Time.fixedDeltaTime * boostDecay);

        // 速度を適用
        rb.linearVelocity = moveVelocity + boostVelocity;

        // 回転を適用（重複削除済み）
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
