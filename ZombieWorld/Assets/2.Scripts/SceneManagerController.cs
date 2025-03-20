using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    public static SceneManagerController Instance { get; private set; }
    string currentSceneName = "Title";

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        SoundManager.Instance.PlayBGM(currentSceneName + "_bgm");
    }

    public void LoadScene(string sceneName)
    {
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySfx("UIClick");
        //몇초 후에 씬이 변경되도록? 코드 넣어야 함?
        SceneManager.LoadScene(sceneName);
        
        //씬 로드 후 해당 씬의 배경음 넣기
        SoundManager.Instance.SetBGMVolume(0.5f);
        SoundManager.Instance.PlayBGM(sceneName + "_bgm");

        Debug.Log("Scene 변경 : " + sceneName);
    }

    public void ExitScene()
    {
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySfx("UIClick");
        Application.Quit();
    }

   
}
