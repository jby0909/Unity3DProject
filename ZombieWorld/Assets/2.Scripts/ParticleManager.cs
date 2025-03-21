using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ParticleType
{
    DamageExplosion,
    WeaponFire,
    WeaponSmoke,

}


public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private Dictionary<ParticleType, GameObject> particleSystemDic = new Dictionary<ParticleType, GameObject>();
    //오브젝트 풀링 변수
    private Dictionary<ParticleType, Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();

    public GameObject weaponExplosionParticle;
    public GameObject weaponFireParticle;
    public GameObject weaponSmokeParticle;

    //오브젝트 풀링할 갯수
    public int poolSize = 30;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        particleSystemDic.Add(ParticleType.DamageExplosion, weaponExplosionParticle);
        particleSystemDic.Add(ParticleType.WeaponFire, weaponFireParticle);
        particleSystemDic.Add(ParticleType.WeaponSmoke, weaponSmokeParticle);

        foreach(var type in particleSystemDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for(int i = 0; i < poolSize; i++)
            {
                GameObject p_obj = Instantiate(particleSystemDic[type]);
                p_obj.gameObject.SetActive(false);
                pool.Enqueue(p_obj);
            }
            particlePools.Add(type, pool);
        }
    }
    
    public void ParticlePlay(ParticleType type, Vector3 position, Vector3 scale)
    {
        ////파티클딕셔너리에 해당 타입이 키로 있을 경우
        //if(particleSystemDic.ContainsKey(type))
        //{
        //    //해당 파티클 인스턴스화(입력받은 위치에, 기본 회전값으로)
        //    ParticleSystem particle = Instantiate(particleSystemDic[type], position, Quaternion.identity);
        //    //파티클 크기 지정
        //    particle.gameObject.transform.localScale = scale;

        //    //파티클 회전
        //    Transform playerTransform = PlayerManager.Instance.transform;
        //    Vector3 directionToPlayer = playerTransform.position - position;
        //    Quaternion rotation = Quaternion.LookRotation(directionToPlayer);

        //    //파티클 플레이
        //    particle.Play();
        //    //파티클 재생된 후 제거
        //    Destroy(particle.gameObject, particle.main.duration);
        //}

        if(particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();
            if(particleObj != null)
            {
                particleObj.transform.position = position;
                
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                //파티클(컴포넌트)이 재생중이라면
                if(particleSystem.isPlaying)
                {
                    //파티클(컴포넌트) 정지
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                //파티클 게임 오브젝트의 크기 지정
                particleObj.transform.localScale = scale;
                particleObj.SetActive(true); //파티클 게임 오브젝트 활성화
                particleSystem.Play();      //파티클(컴포넌트) 재생
                StartCoroutine(particleEnd(type, particleObj, particleSystem)); //파티클 재생시간 끝나면 멈추는 코루틴 실행
            }
        }
    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        //파티클이 재생중일 때는 기다림
        while(particleSystem.isPlaying)
        {
            yield return null;
        }
        //파티클 멈춤
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj);
    }
}
