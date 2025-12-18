using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class GameHUDController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Dependencies")]
    [SerializeField] private LiveCommentary liveCommentary;

    private VisualElement commentaryArea;
    private Label commentaryLabel;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[GameHUDController] UIDocument is missing.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        commentaryArea = root.Q<VisualElement>("CommentaryArea");
        commentaryLabel = root.Q<Label>("CommentaryText");

        // LiveCommentaryの自動検索
        if (liveCommentary == null)
        {
            liveCommentary = FindObjectOfType<LiveCommentary>();
            if (liveCommentary == null)
            {
                Debug.LogWarning("[GameHUDController] LiveCommentary not found in scene.");
            }
        }

        // 実況イベントの購読
        if (liveCommentary != null)
        {
            liveCommentary.OnCommentReceived += ShowCommentary;
        }
    }

    private void OnDisable()
    {
        if (liveCommentary != null)
        {
            liveCommentary.OnCommentReceived -= ShowCommentary;
        }
    }

    public void ShowCommentary(string text)
    {
        if (commentaryLabel == null || commentaryArea == null) return;

        Debug.Log($"[GameHUD] Showing commentary: {text}");

        // テキスト更新
        commentaryLabel.text = text;

        // 表示アニメーション
        StopAllCoroutines(); // 既存のアニメーションをリセット
        StartCoroutine(AnimateCommentary());
    }

    private IEnumerator AnimateCommentary()
    {
        // 表示
        commentaryArea.RemoveFromClassList("commentary-hidden");
        commentaryArea.AddToClassList("commentary-visible");

        // 5秒間表示
        yield return new WaitForSeconds(5f);

        // 非表示
        commentaryArea.RemoveFromClassList("commentary-visible");
        commentaryArea.AddToClassList("commentary-hidden");
    }
}
