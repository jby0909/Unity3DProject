using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    //싱글톤 패턴
    public static SceneManagerController Instance { get; private set; }
    
    string currentSceneName;

    private void Awake()
    {
        //싱글톤 패턴
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
        //현재 Scene이름을 활성화된 Scene의 이름으로 설정 
        currentSceneName = SceneManager.GetActiveScene().name;
        //bgm실행
        SoundManager.Instance.PlayBGM(currentSceneName + "_bgm");
    }

    public void LoadScene(string sceneName)
    {
        //효과음 볼륨조절, 실행
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
        // Unity 에디터에서 실행 중일 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 빌드된 게임에서 종료
#endif
        // 마우스 커서 보이도록 설정 (Exit 후에도 마우스 커서가 보이게)
        Cursor.lockState = CursorLockMode.None; // 마우스를 자유롭게 움직일 수 있도록 설정
        Cursor.visible = true; // 마우스 커서 보이게 설정

        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySfx("UIClick");
        Application.Quit();
    }

   
}
