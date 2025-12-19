using UnityEngine;
using System.Collections;

public class GameBGMManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float fadeDuration = 1.5f; // フェードアウトにかける秒数

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 1.0f; // 初期ボリュームを最大に
        }
        else
        {
            Debug.LogError("<color=red>[GameBGMManager] AudioSource is NOT assigned! Please assign it in the Inspector.</color>");
        }
    }

    // カウントダウン終了時に呼び出し
    public void PlayBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.volume = 1.0f; 
            bgmSource.Play();
            Debug.Log($"<color=green>[GameBGM] BGM Started. Clip: {bgmSource.clip?.name}, Volume: {bgmSource.volume}, Mute: {bgmSource.mute}, Loop: {bgmSource.loop}, SpatialBlend: {bgmSource.spatialBlend}</color>");
        }
    }

    // 終了時（ゴール/タイムアップ）に呼び出し
    public void StopBGMWithFade()
    {
        if (bgmSource == null || !bgmSource.isPlaying) return;

        // 既にフェード中なら二重実行しない
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = bgmSource.volume;
        float timer = 0;

        Debug.Log($"<color=white>[GameBGM] Fading out... ({fadeDuration}s)</color>");

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume; // 次回再生のために戻しておく
        Debug.Log("<color=white>[GameBGM] BGM Stopped.</color>");
    }

    // ゴール時やリタイア時に止める用
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
}