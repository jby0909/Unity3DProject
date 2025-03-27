using UnityEngine;

public class StaticUI : MonoBehaviour
{
    public static StaticUI Instance { get; private set; }
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
