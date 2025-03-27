using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    //싱글톤
    public static SoundManager Instance { get; private set; }

    public AudioSource bgmSource;   //배경음 킬 것
    public AudioSource sfxSource;   //효과음 킬 것

    //딕셔너리로 audioClip 관리. 딕셔너리는 public으로 설정해도 유니티 인스펙터 창에 안 뜬다 
    public Dictionary<string, AudioClip> DicbgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> DicsfxClips = new Dictionary<string, AudioClip>();

    // -> 그래서 이런식으로 구현
    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    private Coroutine currentBGMCoroutine;

    private void Awake()
    {
        //싱글톤
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClip();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    ////씬 바뀔 때 배경음 바꾸기 예시 -> scenemanager에서 관리할수도?
    //private void Start()
    //{
    //    string activeSceneName = SceneManager.GetActiveScene().name;
    //    OnSceneLoaded(activeSceneName);
    //}
    ////씬 바뀔 때 배경음 바꾸기 예시 
    //public void OnSceneLoaded(string sceneName)
    //{
    //    if(sceneName == "GameScene")
    //    {
    //        PlayBGM("GameScene1", 1.0f);
    //    }
    //    else if(sceneName == "GameScene2")
    //    {
    //        PlayBGM("GameScene2", 1.0f);
    //    }
    //}


    void InitializeAudioClip()
    {
        //bgm리스트 받아온것 순회
        foreach(var bgm in bgmClipList)
        {
            //딕셔너리에 해당 키 이름이 없을 때
            if(!DicbgmClips.ContainsKey(bgm.name))
            {
                //딕셔너리에 추가
                DicbgmClips.Add(bgm.name, bgm.clip);
            }            
        }
        foreach(var sfx in sfxClipList)
        {
            if(!DicsfxClips.ContainsKey(sfx.name))
            {
                DicsfxClips.Add(sfx.name, sfx.clip);
            }
        }
    }

    public void PlayBGM(string name, float fadeDuration = 1.0f) // fadeDuration : 페이드 인/아웃 되는데 걸리는 시간
    {
        //딕셔너리에서 해당 이름에 해당하는 키값이 있으면
        if(DicbgmClips.ContainsKey(name))
        {
            //현재 진행중인 배경음악 코루틴이 있으면
            if(currentBGMCoroutine != null)
            {
                //해당 배경음악 코루틴을 멈춤
                StopCoroutine(currentBGMCoroutine);
            }

            //현재 배경음악 코루틴을 FadeOutBGM으로 설정
            currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () =>
            {
                bgmSource.spatialBlend = 0f; // 공간감? 배경음은 2D로 설정
                bgmSource.clip = DicbgmClips[name]; //해당 이름의 audioClip을 배경음 audioClip으로 설정
                bgmSource.Play(); // 플레이
                currentBGMCoroutine = StartCoroutine(FadeInBGM(fadeDuration)); //현재 배경음 코루틴을 FadeIn으로 틀어지게 함
            }));
            //bgmSource의 클립을 해당 딕셔너리의 값(AudioClip)으로 설정 후 플레이
            bgmSource.clip = DicbgmClips[name];
            bgmSource.Play();
        }
    }

    //위치 지정 없이 해당 오디오 소스에서 재생시킬 때
    public void PlaySfx(string name)
    {
        if (DicsfxClips.ContainsKey(name))
        {
            sfxSource.PlayOneShot(DicsfxClips[name]); 
        }
    }
    //특정 위치에서 사운드 재생시킬 때(overload)
    public void PlaySfx(string name, Vector3 position)
    {
        if (DicsfxClips.ContainsKey(name))
        {
            AudioSource.PlayClipAtPoint(DicsfxClips[name], position); // 특정 위치에서 사운드 재생
        }
    }

    //bgm 멈추기
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    //bgm볼륨조절
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    //볼륨 페이드 인/아웃
    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete)
    {
        //시작 볼륨을 현재 배경음 볼륨으로 설정
        float startVolume = bgmSource.volume;
        //설정한 시간(duration) 동안
        for(float t = 0; t < duration; t += Time.deltaTime)
        {
            //배경음 볼륨을 시작 볼륨부터 0까지 t/duration만큼씩 변화시킴(볼륨이 서서히 줄어드는 효과)
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        bgmSource.volume = 0;   //마지막에 볼륨을 0으로 설정(오차가 있을 수 있으므로)
        onFadeComplete?.Invoke(); //페이드 아웃이 완료되면 다음 작업 실행
    }

    private IEnumerator FadeInBGM(float duration)
    {
        float startVolume = 0f;
        bgmSource.volume = 0f;

        for(float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 1f, t / duration);
            yield return null;
        }

        bgmSource.volume = 1.0f;
    }
}
