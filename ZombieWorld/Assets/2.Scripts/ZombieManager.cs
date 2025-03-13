using Unity.VisualScripting;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    //이건 과제용으로 만든 변수. 나중에 삭제할 예정 PlayerManager수정 필요
    private int hp;
    public int Hp
    { 
        get { return hp; }
        set { hp = value; }
    }

    public EZombieState currentState = EZombieState.Idle; //현재상태
    public Transform target; 
    public float attackRange = 1.0f; //공격 범위
    public float attackDelay = 2.0f; //공격 딜레이
    private float nextAttackTime = 0.0f; //다음 공격 시간관리
    public Transform[] patrolPoints; //순찰 경로 지점들
    private int currentPoint = 0; //현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f; 
    private float trackingRange = 3.0f; //추적 범위 설정
    private bool isAttack = false; // 공격 상태
    private float eveadeRange = 5.0f; // 도망 상태 회피 거리
    private float zombieHp = 10.0f;
    private float distanceTotarget; // 타겟과의 거리 계산 값
    private bool isWaiting = false; // 상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f; //각 상태 전환 후 대기 시간

    private void Awake()
    {
        //임시지정
        hp = 10;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        distanceTotarget = Vector3.Distance(transform.position, target.position);
        Debug.Log("distanceTotarget : " + distanceTotarget);
        //타겟과의 거리가 추적범위보다 작으면
        if(distanceTotarget < trackingRange)
        {
            //타겟의 위치쪽으로 이동한다
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            //타겟 방향으로 바라본다
            transform.LookAt(target.position);
            Debug.Log("Player 추적");
        }
        //타겟과의 거리가 공격범위보다 작으면
        else if(distanceTotarget < attackRange)
        {
            //공격
            Debug.Log("Player 공격");
        }
        else
        {
            if(patrolPoints.Length > 0)
            {
                Debug.Log("순찰중");
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.LookAt(target.position);

                if(Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Animator animator = other.GetComponent<Animator>();
        //if (animator)
        //{
        //    animator.SetTrigger("Damage");
        //}

        //if (other.gameObject.CompareTag("Player"))
        //{
        //    other.gameObject.transform.position = new Vector3(0, 0, 0);
        //    //other.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.red;
        //}

       

    }
}

public enum EZombieState
{
    Patrol, //순찰모드
    Chase, //추적모드
    Attack, //공격
    Evade, //도망
    Damage, //피격
    Idle, //대기
    Die, //사망
}
