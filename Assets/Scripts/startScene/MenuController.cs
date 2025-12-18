using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TextMeshPro menuText;
    
    [Header("コースリスト")]
    [SerializeField] private string[] courseNames = { "SUZUKA CIRCUIT", "CHUTORIAL", "MIE" };
    
    [Header("BGM管理")]
    [SerializeField] private BGMManager bgmManager; // 先ほど作成したBGMManagerをアタッチ

    [Header("操作設定")]
    [SerializeField] private KeyCode startKey = KeyCode.Space; // 最終決定キー（例：スペースキー）
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape; // キャンセル・戻るキー

    private enum MenuState {
        WaitingForInput, // 1,2,3の入力を待っている状態
        ReadyToStart     // コースが確定し、開始ボタンを待っている状態（BGM：Heavy）
    }
    
    private MenuState currentState = MenuState.WaitingForInput;
    private int selectedCourseIndex = 0;
    
    void Start()
    {
        if (menuText == null)
        {
            Debug.LogError("Menu Text (TextMeshPro) が設定されていません。");
            return;
        }
        UpdateMenuText();
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
            // 数字キー 1, 2, 3 を MapID として受け取る
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectCourse(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectCourse(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SelectCourse(2);
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

        UpdateMenuText();
    }

    private void UpdateMenuText()
    {
        string displayMessage = "";
        
        if (currentState == MenuState.WaitingForInput)
        {
            displayMessage = "SELECT COURSE\n[1][2][3]";
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
        
        enabled = false;
        SceneManager.LoadScene(sceneToLoad);
    }
}