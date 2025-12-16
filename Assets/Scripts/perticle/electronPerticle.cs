using UnityEngine;

public class ElectronPerticle : MonoBehaviour
{
    private controller controllerScript;
    private ParticleSystem boostParticle;
    private ParticleSystem coldParticle;

    private float lastVoltage = 0f;
    private bool isBoost = false;


    void Awake()
    {
        controllerScript = GetComponent<controller>();
        boostParticle = transform.Find("BoostPerticle").GetComponent<ParticleSystem>();
        coldParticle = transform.Find("ColdPerticle").GetComponent<ParticleSystem>();

        boostParticle.Stop();
        coldParticle.Stop();
    }

    void Update()
    {
        if (!controllerScript.isOnConductor)
        {
            coldParticle.Play();
        }
        else if (coldParticle.isPlaying)
        {
            coldParticle.Stop();
        }

        if(lastVoltage != controllerScript.voltage)
        {
            boostParticle.Play();
        }
        lastVoltage = controllerScript.voltage;

    }

}
