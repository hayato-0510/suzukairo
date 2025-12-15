using UnityEngine;
using TMPro; // TextMesh Proを使用するために必要

public class MenuController : MonoBehaviour
{
    // --- Inspectorで設定 ---
    [Header("UI要素")]
    [SerializeField] private TextMeshPro menuText; // 3D空間に配置したTMPテキストをセット
    
    [Header("コースリスト")]
    [SerializeField] private string[] courseNames = { "SUZUKA CIRCUIT", "CHUTORIAL", "MIE" };
    
    [Header("入力キー設定")]
    [SerializeField] private KeyCode nextKey = KeyCode.JoystickButton0; // コース選択・決定 (例: Aボタン)
    [SerializeField] private KeyCode startKey = KeyCode.JoystickButton7; // ゲーム開始 (例: Startボタン)
    
    // --- 内部状態 ---
    private enum MenuState {
        SelectCourse, // コース選択中
        ReadyToStart // スタート待機中
    }
    
    private MenuState currentState = MenuState.SelectCourse;
    private int selectedCourseIndex = 0;
    
    void Start()
    {
        if (menuText == null)
        {
            Debug.LogError("Menu Text (TextMeshProUGUI) が設定されていません。");
            return;
        }
        UpdateMenuText();
    }

    void Update()
    {
        HandleInput();
    }
    
    // --- 入力処理 ---
    private void HandleInput()
    {
        // 決定/次へキーの処理
        if (Input.GetKeyDown(nextKey))
        {
            if (currentState == MenuState.SelectCourse)
            {
                // コースを切り替える
                selectedCourseIndex = (selectedCourseIndex + 1) % courseNames.Length;
                UpdateMenuText();
            }
            else if (currentState == MenuState.ReadyToStart)
            {
                // ReadyToStart状態での決定キー -> コース選択に戻る
                currentState = MenuState.SelectCourse;
                UpdateMenuText();
            }
        }
        
        // ゲーム開始キーの処理 (Startボタンなど)
        if (Input.GetKeyDown(startKey))
        {
            if (currentState == MenuState.SelectCourse)
            {
                // コース選択完了 -> スタート待機状態へ
                currentState = MenuState.ReadyToStart;
                UpdateMenuText();
            }
            else if (currentState == MenuState.ReadyToStart)
            {
                // スタート待機完了 -> ゲーム開始処理
                StartGame();
            }
        }
    }

    // --- テキスト表示の更新 ---
    private void UpdateMenuText()
    {
        string displayMessage = "";
        
        if (currentState == MenuState.SelectCourse)
        {
            // コース選択中: 現在選択されているコース名と操作説明を表示
            displayMessage = $"COURSE: {courseNames[selectedCourseIndex]}";
            //displayMessage += $"Press [{nextKey.ToString()}] to Change Course";
            //displayMessage += $"Press [{startKey.ToString()}] to Select and Continue";
        }
        else if (currentState == MenuState.ReadyToStart)
        {
            // スタート待機中: 決定したコース名と最終確認を表示
            displayMessage = $"READY TO START\n\n";
            displayMessage += $"Course: {courseNames[selectedCourseIndex]}\n";
            //displayMessage += $"Press [{startKey.ToString()}] to START GAME\n";
            //displayMessage += $"Press [{nextKey.ToString()}] to Return to Selection";
        }
        
        menuText.text = displayMessage;
    }

    // --- ゲーム開始処理 ---
    private void StartGame()
    {
        Debug.Log($"ゲーム開始！選択されたコース: {courseNames[selectedCourseIndex]}");
        
        // ★ここに実際のシーン遷移のロジックを実装します (例: SceneManager.LoadScene(courseNames[selectedCourseIndex]))
        
        // 画面のテキストを最終メッセージに変更
        menuText.text = $"LOADING {courseNames[selectedCourseIndex]}...";
        // スクリプトの入力を無効化
        enabled = false;
    }
}