using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TextMeshPro menuText;
    
    [Header("コースリスト")]
    [SerializeField] private string[] courseNames = { "TUTORIAL", "MIE", "SUZUKA CIRCUIT" };
    
    [Header("BGM管理")]
    [SerializeField] private BGMManager bgmManager; // 先ほど作成したBGMManagerをアタッチ

    [Header("球生成管理")]
    [SerializeField] private SphereSpawner sphereSpawner; // SphereSpawnerへの参照

    [Header("操作設定")]
    [SerializeField] private KeyCode startKey = KeyCode.Space; // 最終決定キー（例：スペースキー）
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape; // キャンセル・戻るキー

    private enum MenuState {
        WaitingForInput, // 1,2,3の入力を待っている状態
        ReadyToStart     // コースが確定し、開始ボタンを待っている状態（BGM：Heavy）
    }
    
    private MenuState currentState = MenuState.WaitingForInput;
    private int selectedCourseIndex = 0;
    private SerialCommandRouter serialRouter;
    
    void Start()
    {
        if (menuText == null)
        {
            Debug.LogError("Menu Text (TextMeshPro) が設定されていません。");
            return;
        }
        UpdateMenuText();
        
        // SerialCommandRouterを検索して取得
        serialRouter = FindObjectOfType<SerialCommandRouter>();
        if (serialRouter == null)
        {
            Debug.LogWarning("[MenuController] SerialCommandRouter not found. Device input will be disabled.");
        }
    }

    void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // 1. コース入力待ち状態
        if (currentState == MenuState.WaitingForInput)
        {
            // キーボード入力を処理
            HandleKeyboardInput();
            // シリアル入力を処理
            HandleSerialInput();
        }
        // 2. 開始待ち状態
        else if (currentState == MenuState.ReadyToStart)
        {
            // スタートキーでゲーム開始
            if (Input.GetKeyDown(startKey))
            {
                StartGame();
            }
            // キャンセルキーでコース選択に戻る
            if (Input.GetKeyDown(cancelKey))
            {
                CancelSelection();
            }
        }
    }

    private void HandleKeyboardInput()
    {
        // 数字キー 1, 2, 3 を インデックス (0, 1, 2) として受け取る
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectCourse(0); // チュートリアル
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectCourse(1); // 三重
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SelectCourse(2); // 鈴鹿
    }
    
    private void HandleSerialInput()
    {
        if (serialRouter == null || serialRouter.mapID == 0) return;

        int deviceMapId = serialRouter.mapID;
        
        // mapIDをリセットして、毎フレーム選択されるのを防ぐ
        serialRouter.mapID = 0;
        
        // deviceMapId (1, 2, 3) をインデックス (0, 1, 2) に変換
        int courseIndex = deviceMapId - 1;

        if (courseIndex >= 0 && courseIndex < courseNames.Length)
        {
            SelectCourse(courseIndex);
        }
        else
        {
            Debug.LogWarning($"[MenuController] Received unknown mapID from device: {deviceMapId}");
        }
    }

    // コースが入力された時の処理
    private void SelectCourse(int index)
    {
        Debug.Log($"Course Selected");
        if (index >= courseNames.Length) return;

        selectedCourseIndex = index;
        currentState = MenuState.ReadyToStart;
        
        // BGMをHeavy（ドラムあり）に切り替え
        if (bgmManager != null)
        {
            bgmManager.SwitchToHeavyLoop();
            Debug.Log($"Course Selected: {courseNames[index]}. BGM Switched to Heavy.");
        }

        // 追加の球を1つ生成（選択されたコースのインデックスを渡す）
        if (sphereSpawner != null)
        {
            Debug.Log($"[MenuController] SpawnAdditionalSphere呼び出し - courseIndex: {index}");
            // 既存の球を全て削除してから新しい球を生成
            sphereSpawner.ClearAllSpheres();
            sphereSpawner.SpawnAdditionalSphere(index);
        }
        else
        {
            Debug.LogWarning("[MenuController] SphereSpawnerが設定されていません！");
        }

        UpdateMenuText();
    }

    // 選択をキャンセルして戻る処理
    private void CancelSelection()
    {
        currentState = MenuState.WaitingForInput;

        // BGMをNormal（ドラム少）に戻す
        if (bgmManager != null)
        {
            bgmManager.SwitchToNormalLoop();
        }

        // 球を初期状態に戻す
        if (sphereSpawner != null)
        {
            sphereSpawner.ResetToInitialSpheres();
        }

        UpdateMenuText();
    }

    private void UpdateMenuText()
    {
        string displayMessage = "";
        
        if (currentState == MenuState.WaitingForInput)
        {
            displayMessage = "SELECT COURSE\n";
            for (int i = 0; i < courseNames.Length; i++)
            {
                // Display 1-based index for user
                displayMessage += $"[{i + 1}] {courseNames[i]}\n";
            }
        }
        else if (currentState == MenuState.ReadyToStart)
        {
            displayMessage = $"READY?\n\n<color=#FFD700>{courseNames[selectedCourseIndex]}</color>\n\nPRESS [SPACE] TO START";
        }
        
        menuText.text = displayMessage;
    }

    private void StartGame()
    {
        // 実際には各コースに応じたシーンをロードする設定も可能
        string sceneToLoad = "SampleScene"; 
        menuText.text = $"LOADING\n{courseNames[selectedCourseIndex]}...";

        // 選択されたコースのIDを保存
        CourseSelectionData.SelectedMapId = selectedCourseIndex;
        
        enabled = false;
        SceneManager.LoadScene(sceneToLoad);
    }
}