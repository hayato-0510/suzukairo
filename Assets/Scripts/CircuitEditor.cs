using UnityEngine;

public class CircuitEditor : MonoBehaviour
{
    [SerializeField] private TextAsset jsonFile;
    public Circuit data;

    void Start()
    {
        data = JsonUtility.FromJson<Circuit>(jsonFile.text);
    }

    void Update()
    {
        
    }
}
