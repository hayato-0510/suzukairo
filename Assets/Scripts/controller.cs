using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    private float _voltage = 5.0f;
    public float voltage
    {
        get { return _voltage; }
        set
        {
            _voltage = value;
            UpdateVoltageNeedle();
        }
    }
    public TextMeshProUGUI voltageText;
    public TextMeshProUGUI ampereText;
    public Transform voltageNeedle;
    public Transform ampereNeedle;
    public Image ampereMeter;
    public float maxVoltage = 10f;
    public float maxAmpere = 1f;
    public float minVoltageAngle = 180f;  // 0Vの時の角度
    public float maxVoltageAngle = 0f;    // maxVoltageの時の角度
    public float minAmpereAngle = 180f;   // 0Aの時の角度
    public float maxAmpereAngle = 0f;     // maxAmpereの時の角度
    public float ampereSmoothSpeed = 5f;  // アンペア表示の滑らかさ（大きいほど速く追従）
    private float displayAmpere = 0f;     // 表示用のアンペア値（滑らかに変化）

    [Header("���M�X�s�[�h")]
    public float radiationSpeed=1.0f;

    private Rigidbody2D rb;
    private Vector3 dir;
    private bool isOnConductor=false;
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
        displayAmpere = startAmpere;  // 表示用アンペアも初期化
        startScale = transform.localScale;
        
        // 電圧計の針を初期位置に設定
        UpdateVoltageNeedle();
        // 電流計の針を初期位置に設定
        UpdateAmpereNeedle();
    }
    
    private void UpdateVoltageNeedle()
    {
        if (voltageNeedle != null && voltageNeedle.gameObject != null)
        {
            float voltageRatio = Mathf.Clamp01(_voltage / maxVoltage);
            float needleAngle = Mathf.Lerp(minVoltageAngle, maxVoltageAngle, voltageRatio);
            Debug.Log($"Voltage: {_voltage}V, Ratio: {voltageRatio}, Angle: {needleAngle}, Min: {minVoltageAngle}, Max: {maxVoltageAngle}");
            voltageNeedle.localRotation = Quaternion.Euler(0, 0, needleAngle);
        }
    }
    
    private void UpdateAmpereNeedle()
    {
        if (ampereNeedle != null && ampereNeedle.gameObject != null)
        {
            // 滑らかに変化させる
            displayAmpere = Mathf.Lerp(displayAmpere, ampere, Time.deltaTime * ampereSmoothSpeed);
            
            float ampereRatio = Mathf.Clamp01(displayAmpere / maxAmpere);
            float needleAngle = Mathf.Lerp(minAmpereAngle, maxAmpereAngle, ampereRatio);
            ampereNeedle.localRotation = Quaternion.Euler(0, 0, needleAngle);
        }
    }

    private void Update()
    {
        voltageText.text = _voltage.ToString("f"+1)+"V";
        ampereText.text = ampere.ToString("f"+3)+"A";
        
        // 電圧計の針を更新
        UpdateVoltageNeedle();
        
        // 電流計の針を更新（滑らかに変化）
        UpdateAmpereNeedle();
        
        // 電流計のゲージを更新
        if (ampereMeter != null)
        {
            ampereMeter.fillAmount = Mathf.Clamp01(ampere / maxAmpere);
        }
        
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
