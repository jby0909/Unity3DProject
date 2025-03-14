using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    private float evadeRange = 5.0f; // 도망 상태 회피 거리
    private float zombieHp = 100.0f;
    private float distanceTotarget; // 타겟과의 거리 계산 값
    private bool isWaiting = false; // 상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f; //각 상태 전환 후 대기 시간
    private Coroutine stateCoroutine; //현재 실행중인 코루틴

    Animator animator;
    private AudioSource audioSource;
    public AudioClip audioClipScream;
    public float animationSpeed = 1.0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        //임시지정
        hp = 10;
    }

    void Start()
    {
        //상태 초기화
        distanceTotarget = Vector3.Distance(transform.position, target.position);
        currentState = EZombieState.Idle;
        if (currentState == EZombieState.Idle)
        {
            stateCoroutine = StartCoroutine(Idle());
        }
        else if (currentState == EZombieState.Patrol)
        {
            stateCoroutine = StartCoroutine(Patrol());
        }


    }


    void Update()
    {
        distanceTotarget = Vector3.Distance(transform.position, target.position);
        animator.speed = animationSpeed;


    }

    public void ChangeState(EZombieState newState)
    {
        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
        }
        currentState = newState;

        switch (currentState)
        {
            case EZombieState.Idle:
                stateCoroutine = StartCoroutine(Idle());
                break;
            case EZombieState.Patrol:
                stateCoroutine = StartCoroutine(Patrol());
                break;
            case EZombieState.Chase:
                stateCoroutine = StartCoroutine(Chase());
                break;
            case EZombieState.Attack:
                stateCoroutine = StartCoroutine(Attack());
                break;
            case EZombieState.Evade:
                stateCoroutine = StartCoroutine(Evade());
                break;
            case EZombieState.Die:
                stateCoroutine = StartCoroutine(Die());
                break;
        }
    }

    private IEnumerator Idle()
    {
        Debug.Log(gameObject.name + " : 대기중");
        animator.SetBool("isWalk", false);



        while (currentState == EZombieState.Idle)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < trackingRange)
            {
                //현재 Idle() 코루틴을 멈추고 Chase()코루틴을 실행하도록 바꾸는 부분
                ChangeState(EZombieState.Chase);
            }
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }

            yield return null;
        }
    }


    private IEnumerator Patrol()
    {
        Debug.Log(gameObject.name + " : 순찰중");
        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                animator.SetBool("isWalk", true);
                animationSpeed = 2.0f; // 걸을 때 애니메이션이 유난히 느려서 속도 올려줌
                //순찰 지점의 위치를 타겟 위치로 지정
                Transform targetPoint = patrolPoints[currentPoint];
                //현재위치로부터 타겟위치까지의 방향 지정
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                //위치 이동
                transform.position += direction * moveSpeed * Time.deltaTime;
                //바라보는 방향 변경
                transform.LookAt(targetPoint.transform);

                //현재위치와 타겟위치까지 거리가 0.3보다 작으면
                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    //순찰지점배열의 인덱스를 하나 늘림
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;

                }


                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < trackingRange)
                {
                    if (distance < attackRange)
                    {
                        ChangeState(EZombieState.Attack);
                    }
                    else
                    {
                        ChangeState(EZombieState.Chase);
                    }
                }

            }
            yield return null;
        }
    }

    private IEnumerator Chase()
    {
        Debug.Log(gameObject.name + " : 플레이어 추적중");

        while (currentState == EZombieState.Chase)
        {
            //현재위치와 타겟위치의 거리
            float distance = Vector3.Distance(transform.position, target.position);
            //현재위치 -> 타겟위치 방향
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(target.transform);
            animator.SetBool("isWalk", true);
            animationSpeed = 2.0f; // 걸을 때 애니메이션이 유난히 느려서 속도 올려줌

            //이부분이 왜 이렇게 고쳤을 때 되는지 생각해보기(아직 이해가 잘 안됨. 왜 되지??)
            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            else if (distance > trackingRange)
            {
                ChangeState(EZombieState.Idle);
            }



            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " : 플레이어 공격");
        transform.LookAt(target.position);
        animator.SetTrigger("Attack");
        animationSpeed = 1.0f; // 공격할 때 애니메이션 본래속도로

        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position, target.position);
        //공격범위 벗어나면
        if (distance > attackRange)
        {
            //추적모드로 전환
            Debug.Log("공격->추적 바뀜");
            ChangeState(EZombieState.Chase);
        }
        else
        {
            //범위내에 있으면 공격모드
            ChangeState(EZombieState.Attack);
        }
    }

    private IEnumerator Evade()
    {
        Debug.Log(gameObject.name + " : 도망중");
        animator.SetBool("isWalk", true);
        animationSpeed = 2.0f; // 걸을 때 애니메이션이 유난히 느려서 속도 올려줌
        //타겟위치 -> 현재위치 방향(타겟과 반대방향으로 도망감)
        Vector3 evadeDirection = (transform.position - target.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0.0f;

        //반대방향으로 쳐다보기(회전)
        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        transform.rotation = targetRotation;

        while (currentState == EZombieState.Evade && timer < evadeTime)
        {
            transform.position += evadeDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            //
            yield return null;
        }

        //도망이 끝나면 대기하거나 순찰하거나 선택해서 코드 수정
        ChangeState(EZombieState.Idle);
    }

    public void TakeDamage(float damage)
    {
        //무적상태, .... 추가

        Debug.Log(gameObject.name + " : " + damage + " 데미지 받음");
        animator.SetTrigger("Damage");
        zombieHp -= damage;


        if (zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            //데미지를 받았을 때 할 부분
            ChangeState(EZombieState.Chase);
        }

    }

    private IEnumerator Die()
    {
        Debug.Log(gameObject.name + " : 사망");
        animator.SetTrigger("Die");
        //2초뒤에 사라짐
        yield return new WaitForSeconds(2.0f);
        //죽음 상태에 대해서 커스텀할 부분
        gameObject.SetActive(false);
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
