using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopNormalSource; // ドラムなし (Normal)
    [SerializeField] private AudioSource loopHeavySource;  // ドラムあり (Heavy)

    private bool isIntroPlaying = true;

    private void Start()
    {
        // 1. 初期設定: 全て無音にする
        introSource.volume = 1.0f;
        loopNormalSource.volume = 0f;
        loopHeavySource.volume = 0f;

        // 2. ループ設定
        introSource.loop = false;
        loopNormalSource.loop = true;
        loopHeavySource.loop = true;

        // 3. 全てのSourceを同時に再生開始（同期を完璧にするため）
        // ループ側は音量0の状態で裏で回し続けておく
        double startTime = AudioSettings.dspTime + 0.5; // 0.5秒後に一斉開始
        introSource.PlayScheduled(startTime);
        loopNormalSource.PlayScheduled(startTime);
        loopHeavySource.PlayScheduled(startTime);

        StartCoroutine(MonitorIntro());
    }

    private IEnumerator MonitorIntro()
    {
        // イントロが再生されている間待機
        yield return new WaitWhile(() => introSource.isPlaying);
        
        isIntroPlaying = false;

        // イントロが終わった瞬間、デフォルト（Normal）の音を出す
        // すでに選択済みでReadyToStart状態ならHeavyを出す、といった制御も可能
        if (loopHeavySource.volume <= 0) 
        {
            loopNormalSource.volume = 1.0f;
        }
    }

    // Heavy（ドラムあり）に即座に切り替え
    public void SwitchToHeavyLoop()
    {
        Debug.Log("<color=orange>[BGM] SwitchToHeavyLoop called. Volume: Normal -> 0, Heavy -> 1</color>");
        loopNormalSource.volume = 0f;
        loopHeavySource.volume = 1.0f;
        
        // もしまだイントロ中なら、イントロを止めて即座にHeavyにする
        if (isIntroPlaying)
        {
            StopAllCoroutines();
            introSource.Stop();
            isIntroPlaying = false;
        }
    }

    // Normal（ドラムなし）に即座に切り替え
    public void SwitchToNormalLoop()
    {
        loopNormalSource.volume = 1.0f;
        loopHeavySource.volume = 0f;
    }
}