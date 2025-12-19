using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public float moveDecay = 3f;
    private Vector2 moveVelocity;

    [Header("電流")]
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

    [Header("放射スピード")]
    public float radiationSpeed=1.0f;

    [Header("チェックポイントテレポート")]
    public float checkPointTPtime = 3.0f;
    public float minAmpere = 0.01f;
    private float tpTime = 0;
    private bool firstTimeShortage = true;

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
    
    // --- ロギング用変数 ---
    private ComponentContext currentComponentContext;
    private float logTimer;
    private bool isTouchingComponent;

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

        // SerialCommandRouter を取得
        serialRouter = FindObjectOfType<SerialCommandRouter>();
        
        // ロギング用コンテキストを初期化
        InitializeComponentContext();
    }
    
    private void InitializeComponentContext()
    {
        currentComponentContext = new ComponentContext
        {
            onComponent = false,
            componentType = "None",
            componentId = -1,
            componentValue = 0f
        };
    }

    private void Update()
    {
        voltageText.text = _voltage.ToString("f"+1)+"V";
        ampereText.text = ampere.ToString("f"+3)+"A";

        GetComponent<LapCounter>().getNextCheckpoint().GetChild(0).gameObject.SetActive(true);
        GetComponent<LapCounter>().getCheckpoint().GetChild(0).gameObject.SetActive(false);

        if (ampere < minAmpere)
        {
            if (firstTimeShortage) 
            { 
                tpTime = 0; 
                firstTimeShortage = false;
            }
            tpTime += Time.deltaTime;
            if (checkPointTPtime <= tpTime)
            {
                transform.position = GetComponent<LapCounter>().getCheckpoint().position;
                ampere = startAmpere/1.5f;
                firstTimeShortage=true;
            }
        }

        
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
    
    private void FixedUpdate()
    {
        if (!canMove) return;

        // isTouchingComponentフラグが更新されなかった場合（=コンポーネントから離れたフレーム）、コンテキストをリセット
        if (!isTouchingComponent)
        {
            InitializeComponentContext();
        }
        isTouchingComponent = false; // 次のFixedUpdateのためにリセット

        // 定期的なログ記録
        if (GameLogger.Instance == null || !GameLogger.Instance.enableLogging) return;
        
        logTimer += Time.fixedDeltaTime;
        if (logTimer >= GameLogger.Instance.loggingInterval)
        {
            logTimer = 0f;
            LogPlayerState();
        }
    }

    private void LogPlayerState()
    {
        var stateData = new PlayerStateData
        {
            position = new Vector2Serializable(transform.position),
            angle = transform.eulerAngles.z,
            velocity = new Vector2Serializable(rb.linearVelocity),
            ampere = this.ampere,
            voltage = this.voltage,
            componentContext = this.currentComponentContext
        };
        GameLogger.Instance.LogPlayerState(stateData);
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
            
            // ブーストイベントをログに記録
            if (GameLogger.Instance != null && GameLogger.Instance.enableLogging)
            {
                GameLogger.Instance.LogBoost(new BoostActivatedData { remainingVoltage = this.voltage });
            }
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
            
            // ブーストイベントをログに記録
            if (GameLogger.Instance != null && GameLogger.Instance.enableLogging)
            {
                GameLogger.Instance.LogBoost(new BoostActivatedData { remainingVoltage = this.voltage });
            }
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
        if (collision.gameObject.CompareTag("conductor"))
        {
            moveSpeed = nonconductorMoveSpeed;
            isOnConductor = false;

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("conductor"))
        {
            moveSpeed = conducutorMoveSpeed;
            isOnConductor = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("conductor"))
        {
            moveSpeed = conducutorMoveSpeed;
            isOnConductor = true;
            
            // ログ用のコンポーネント情報を更新
            var componentData = collision.gameObject.GetComponent<componentData>();
            if (componentData != null)
            {
                currentComponentContext.onComponent = true;
                currentComponentContext.componentType = componentData.type;
                currentComponentContext.componentId = componentData.id;
                currentComponentContext.componentValue = componentData.value;
            }
            else
            {
                // componentDataがない導体（例：ただの線）の場合
                currentComponentContext.onComponent = true;
                currentComponentContext.componentType = "Conductor"; // デフォルトタイプ
                currentComponentContext.componentId = -1;
                currentComponentContext.componentValue = 0;
            }
            isTouchingComponent = true;
        }
    }
}
