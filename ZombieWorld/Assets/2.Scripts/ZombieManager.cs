using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class ZombieManager : MonoBehaviour
{
   
    public EZombieState currentState = EZombieState.Idle; //현재상태
    public float attackRange = 1.0f; //공격 범위
    public float attackDelay = 2.0f; //공격 딜레이
    private float nextAttackTime = 0.0f; //다음 공격 시간관리
    public Transform[] patrolPoints; //순찰 경로 지점들
    private int currentPoint = 0; //현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f;
    private float trackingRange = 5.0f; //추적 범위 설정
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
    public float animationWalkSpeed = 2.0f;

    private NavMeshAgent agent;

    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;
    public float jumpDuration = 1.0f;
    private NavMeshLink[] navMeshLinks;

    public static int ZombieCount = 0; //현재 scene에 존재하는 총 좀비의 수

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsOfType<NavMeshLink>();
       
    }

    void Start()
    {
        ZombieCount++; // 시작할 때 좀비 수 하나 늘리기
        //상태 초기화
        distanceTotarget = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        //currentState = EZombieState.Idle;
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
        
        distanceTotarget = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        //animator.speed = 1.0f;


    }

    public void ChangeState(EZombieState newState)
    {
        //점프했을 때 다른것을 하지 못하게 막는 코드
        if (isJumping) return;

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
        animator.speed = 1.0f;
        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);
        agent.isStopped = true;

        yield return null;

        //while (currentState == EZombieState.Idle)
        //{
        //    float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

        //    if (distance < trackingRange)
        //    {
        //        //현재 Idle() 코루틴을 멈추고 Chase()코루틴을 실행하도록 바꾸는 부분
        //        ChangeState(EZombieState.Chase);
        //    }
        //    else if (distance < attackRange)
        //    {
        //        ChangeState(EZombieState.Attack);
        //    }

        //    yield return null;
        //}
    }


    private IEnumerator Patrol()
    {
        Debug.Log(gameObject.name + " : 순찰중");
        animator.speed = animationWalkSpeed;
        currentPoint = Random.Range(0, patrolPoints.Length);
        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                animator.SetBool("isWalk", true);
                animator.SetBool("isRun", false);
                
                //순찰 지점의 위치를 타겟 위치로 지정
                Transform targetPoint = patrolPoints[currentPoint];
                //현재위치로부터 타겟위치까지의 방향 지정
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                //위치 이동
                //transform.position += direction * moveSpeed * Time.deltaTime;
                //바라보는 방향 변경
                //transform.LookAt(targetPoint.transform);
                agent.speed = moveSpeed;
                agent.isStopped = false;    //멈출지 여부
                agent.destination = targetPoint.position; //목적지

                //nav mesh link에 가까워지면?
                if(agent.isOnOffMeshLink)
                {
                    //뭔가 행동할 것 추가
                    StartCoroutine(JumpAcrossLink());
                }

                //현재위치와 타겟위치까지 거리가 1.2보다 작으면
                if (Vector3.Distance(transform.position, targetPoint.position) < 1.2f)
                {
                    //순찰지점배열의 인덱스를 하나 늘림
                    //currentPoint = (currentPoint + 1) % patrolPoints.Length;

                    //순찰지점배열의 인덱스를 랜덤으로
                    currentPoint = Random.Range(0, patrolPoints.Length);

                }


                float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
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
        SoundManager.Instance.PlaySfx("DefaultZombie", transform.position);
        animator.speed = 1.0f;

        while (currentState == EZombieState.Chase)
        {
            //현재위치와 타겟위치의 거리
            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
            //플레이어 이동
            //현재위치 -> 타겟위치 방향
            Vector3 direction = (PlayerManager.Instance.transform.position - transform.position).normalized;
            agent.speed = moveSpeed * 2;
            agent.destination = PlayerManager.Instance.transform.position; //목적지
            agent.isStopped = false;    //멈출지 여부
            //transform.position += direction * moveSpeed * Time.deltaTime;
            //transform.LookAt(target.transform);

            animator.SetBool("isRun", true);
            animator.SetBool("isWalk", false);


            //이부분이 왜 이렇게 고쳤을 때 되는지 생각해보기(아직 이해가 잘 안됨. 왜 되지??)
            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            else if (distance > trackingRange)
            {
                ChangeState(EZombieState.Patrol);
            }



            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " : 플레이어 공격");
        animator.speed = 1.0f;
        //바라보기
        //transform.LookAt(target.position);
        //while(/*바라보고 있지 않을 때*/)
        //{
        //    agent.destination = target.position; //목적지
        //    agent.speed = moveSpeed;
        //    agent.isStopped = false;    
        //}

        agent.isStopped = true; // 바라보고 나서는 멈춤
        animator.SetTrigger("Attack");
        //audioSource.PlayOneShot(audioClipScream);
        SoundManager.Instance.PlaySfx("ScreamZombie", transform.position);

        yield return new WaitForSeconds(attackDelay);

        

        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        if (!PlayerManager.Instance.isLive)
        {
            ChangeState(EZombieState.Idle);
        }
        //공격범위 벗어나면
        else if (distance > attackRange)
        {
            //추적모드로 전환
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
        animator.speed = animationWalkSpeed;
        animator.SetBool("isWalk", true);
        animator.SetBool("isRun", false);

        //타겟위치 -> 현재위치 방향(타겟과 반대방향으로 도망감)
        Vector3 evadeDirection = (transform.position - PlayerManager.Instance.transform.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0.0f;

        //반대방향으로 쳐다보기(회전)
        Quaternion targetRotation = Quaternion.LookRotation(evadeDirection);
        //transform.rotation = targetRotation;
        
        agent.destination = transform.position + evadeDirection * 10f ; //목적지
        agent.speed = moveSpeed;
        agent.isStopped = false;

        while (currentState == EZombieState.Evade && timer < evadeTime)
        {
            //transform.position += evadeDirection * moveSpeed * Time.deltaTime;

            if(Vector3.Distance(agent.destination, transform.position) < 1.2f)
            {
                ChangeState(EZombieState.Idle);
                //코루틴 즉시 종료
                yield break;
            }

            timer += Time.deltaTime;
            //
            yield return null;
        }
        ChangeState(EZombieState.Idle);
        //도망이 끝나면 대기하거나 순찰하거나 선택해서 코드 수정
    }

    public void TakeDamage(float damage)
    {
        //무적상태, .... 추가

        Debug.Log(gameObject.name + " : " + damage + " 데미지 받음");
        animator.speed = 1.0f;
        animator.SetTrigger("Damage");
        zombieHp -= damage;


        if (zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
            //더이상 총을 맞지 않게 콜라이더를 끔
            GetComponent<CapsuleCollider>().enabled = false;
        }
        else
        {
            //데미지를 받았을 때 할 부분
            ChangeState(EZombieState.Chase);
        }

    }

    private IEnumerator Die()
    {
        ZombieCount--; // 죽으면 좀비 수 줄이기
        Debug.Log(gameObject.name + " : 사망");
        animator.speed = 1.0f;
        animator.SetTrigger("Die");
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        //2초뒤에 사라짐
        yield return new WaitForSeconds(2.0f);
        //죽음 상태에 대해서 커스텀할 부분
        gameObject.SetActive(false);
    }

    private IEnumerator JumpAcrossLink()
    {
        Debug.Log(gameObject.name + " 좀비 점프");

        isJumping = true;

        agent.isStopped = true;

        //NavMeshLink의 시작과 끝 좌표 가져오기
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        //점프 경로 계산(포물선을 그리며 점프)
        float elapsedTime = 0;
        while(elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; //포물선 경로
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //도착점에 위치(약간 어긋날 수 도 있기 때문에, 공중에서 도착지점을 찾는것을 멈췄기 때문에, 오차가 생김) -> 도착점을 정확하게 넣어준 뒤 다음 동작을 하도록 만들어야 함
        transform.position = endPos;
        //NavMeshAgent 경로 재개
        agent.CompleteOffMeshLink(); //우리가 지정한 포물선 경로는 기존의 경로를 우리가 임의로 이동시킨 것이기 때문에 원래의 경로를 다시 가도록 설정해주는 것?
        agent.isStopped = false;
        isJumping = false;
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
