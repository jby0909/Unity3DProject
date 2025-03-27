using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class SceneManagerController : MonoBehaviour
{
    //싱글톤 패턴
    public static SceneManagerController Instance { get; private set; }

    public Image panel;
    public float fadeDuration = 1.0f;
    private bool isFading = false;

    string currentSceneName;

    private GameObject currentUI;

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
        SoundManager.Instance.bgmSource.clip = SoundManager.Instance.DicbgmClips[currentSceneName + "_bgm"];
        SoundManager.Instance.bgmSource.Play();
    }

    private void FindPanel()
    {
        panel = GameObject.Find("Canvas").transform.Find("FadePanel").GetComponent<Image>();
        //만약 panel이 없을경우 예외처리 생각하기
    }

    private void UpdateCurrentUI()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("현재씬 : " + currentSceneName);
        currentUI = GameObject.Find("UI").transform.Find(currentSceneName + "UI").gameObject;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("유아이업데이트 이벤트");
        UpdateCurrentUI();
        currentUI.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("LoadScene에 들어옴");
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        //효과음 볼륨조절, 실행
        SoundManager.Instance.SetSFXVolume(0.3f);
        SoundManager.Instance.PlaySfx("UIClick");
        
        
        //몇초 후에 씬이 변경되도록? 코드 넣어야 함?
        StartCoroutine(FadeInAndLoadScene(sceneName));
        
        
        //씬 로드 후 해당 씬의 배경음 넣기 -> 로드 후라고 확정할 수 없음 -> SceneManager.Load()가 원래 비동기적으로 실행하는 함수라서?
        SoundManager.Instance.SetBGMVolume(0.3f);
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

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G) && !isFading)
        //{
        //    StartCoroutine(FadeInAndLoadScene());
        //}
    }

    IEnumerator FadeInAndLoadScene(string sceneName)
    {
        Debug.Log("페이드인/아웃 & 씬 로드 코루틴");
        isFading = true;

        yield return StartCoroutine(FadeImage(0,1,fadeDuration, sceneName));
        UpdateCurrentUI();
        currentUI.SetActive(false);

        isFading = false;

        yield return StartCoroutine(FadeImage(1, 0, fadeDuration, sceneName));
        

    }

    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration, string sceneName)
    {
        Debug.Log("진짜 페이드인 코루틴");
        Debug.Log("isFading 상태 : " + isFading);
        panel.gameObject.SetActive(true);
        float elapsedTime = 0.0f;
        Color panelColor = panel.color;
        
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            panelColor.a = newAlpha;
            panel.color = panelColor;
            yield return null;
        }
        panelColor.a = endAlpha;
        panel.color = panelColor;
        panel.gameObject.SetActive(false);
        if (isFading)
        {
            Debug.Log("바뀔 씬 이름 : " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
   
}
