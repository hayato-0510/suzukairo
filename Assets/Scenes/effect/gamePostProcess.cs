using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GamePostProcess : MonoBehaviour
{
    [SerializeField] controller controllerScript;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float vignetteMin = 0.3f;
    [SerializeField] private float vignetteMax = 0.6f;

    [SerializeField] private float vignetteSpeed = 1f;
    private Vignette vignette;
    private float vignetteTarget = 0f;
    private float currentValue;

    void Awake()
    {
        globalVolume.profile.TryGet(out vignette);
        vignette.intensity.overrideState = true;
        vignette.active = false;
    }

    void Update()
    {
        if (!controllerScript.isOnConductor)
        {
            vignette.active = true;
            float sin = Mathf.Sin(Time.time * vignetteSpeed) + 1f;
            vignetteTarget = Mathf.Lerp(vignetteMin, vignetteMax, sin);
        }
        else
        {
            vignetteTarget = 0;
            vignette.active = false;
        }

        if (vignette.intensity.value != vignetteTarget)
        {
            currentValue = Mathf.Lerp(currentValue, vignetteTarget, Time.deltaTime * vignetteSpeed);
            vignette.intensity.value = currentValue;
        }
    }

}
