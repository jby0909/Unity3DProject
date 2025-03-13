using System;
using System.Collections;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Animations.Rigging; // NameSpace : 소속


public class PlayerManager : MonoBehaviour
{
    private float moveSpeed = 5.0f; //플레이어 이동 속도
    public float mouseSensitivity = 100.0f; // 마우스 감도
    public Transform cameraTransform; // 카메라의 Transform
    public CharacterController characterController;
    public Transform playerHead; //플레이어 머리 위치(1인칭 모드를 위해서)
    public float thirdPersonDistance = 3.0f; // 3인칭 모드에서 플레이어와 카메라의 거리
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); //3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; //플레이어 시야 위치

    public float zoomeDistance = 1.0f; //카메라가 확대될때의 거리(3인칭 모드에서 사용)
    public float zoomSpeed = 5.0f; //확대축소가 되는 속도
    public float defaultFov = 60.0f; //기본 카메라 시야각
    public float zoomeFov = 30.0f; //확대 시 카메라 시야각(1인칭 모드에서 사용)

    private float currentDistance; //현재 카메라와의 거리(3인칭 모드)
    private float targetDistance; //목표 카메라 거리
    private float targetFov; //목표 FOV
    private bool isZoomed = false; // 확대 여부 확인
    private Coroutine zoomCoroutine; //코루틴을 사용하여 확대 축소 처리
    private Camera mainCamera; //카메라 컴포넌트

    private float pitch = 0.0f; //위아래 회전 값
    private float yaw = 0.0f; //좌우 회전값
    private bool isFirstPerson = false; //1인칭 모드 여부
    private bool isRotaterAroundPlayer = true; //카메라가 플레이어 주위를 회전하는지 여부

    //중력 관련 변수
    public float gravity = -9.81f; //CharacterController에서는 중력이 적용안돼서 직접 설정해준다?
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround; //땅에 닿았는지 여부

    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunning = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    private bool isAim = false;
    private bool isFire = false;

    //사운드 관련 변수
    public AudioClip audioClipFire;
    private AudioSource audioSource;
    public AudioClip audioClipWeaponChange;
    public AudioClip audioClipPickUp;
    public GameObject RifleM4Obj;

    private int animationSpeed = 1;
    private string currentAnimation = "Idle";

    public Transform aimTarget;

    private float weaponMaxDistance = 100.0f; //총의 사정거리

    public LayerMask TargetLayerMask; //감지(탐색)할 레이어

    //MultiAimconstraint컴포넌트 사용하기(런타임에서 궤적그리기)
    public MultiAimConstraint multiAimConstraint;

    //아이템 줍기
    public Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    //화면의 이미지(무기 획득 시 활성화) 변수
    public GameObject crosshairObj;
    public GameObject weaponIconObj;

    bool isGetWeapon = false; //무기를 획득했을때만 조준/무기변경 가능하게 하기 위한 변수
    bool isUseWepon = false; //무기를 꺼냈는지 여부

    public ParticleSystem WeaponEffect; // 파티클(총 쏠 때 효과)

    private float rifleFireDelay = 0.5f;
    

    //총알 변수(임시)
    int bulletCount = 10;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        RifleM4Obj.SetActive(false);
        crosshairObj.SetActive(false);
        weaponIconObj.SetActive(false);
    }

    void MouseSet()
    {
        //마우스 입력을 받아 카메라가 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        //각도 제한(3인칭게임에서 보통 -45 ~ 45 도 정도 쓴다) 
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        isGround = characterController.isGrounded;


        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    
    void CameraSet()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭 모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어의 시야에 따라서 회전합니다.");
        }

        

    }
    void PlayerMovement()
    {
        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }
    }

    void AimSet()
    {
        //개인 해석
        if (Input.GetMouseButtonDown(1) && isGetWeapon && isUseWepon) //마우스 오른쪽 버튼 눌렀을 때
        {
            isAim = true;
            crosshairObj.SetActive(true);
            //???????
            multiAimConstraint.data.offset = new Vector3(-50, 0, 0);
            //animator.SetBool("isAim", isAim);
            animator.SetLayerWeight(1, 1); // 1번 레이어를 1로 활성화


            if (zoomCoroutine != null) // 코루틴이 이미 작동중일 때
            {
                StopCoroutine(zoomCoroutine); //해당 코루틴을 멈춘다
            }

            if (isFirstPerson) //1인칭 시점이면 -> 카메라 자체의 줌기능
            {
                SetTargetFOV(zoomeFov); //확대시 시야각을 목표 시야각으로 설정
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); //줌을 진행할 코루틴을 실행

            }
            else // 3인칭 시점이면 -> 카메라의 위치 이동
            {
                SetTargetDistance(zoomeDistance); // 확대시 거리를 목표 거리로 설정
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance)); //줌을 진행할 코루틴을 실행
            }
        }

        if (Input.GetMouseButtonUp(1) && isGetWeapon && isUseWepon) //마우스 오른쪽 버튼 뗐을 때
        {
            isAim = false;
            crosshairObj.SetActive(false);
            multiAimConstraint.data.offset = new Vector3(0, 0, 0);
            //animator.SetBool("isAim", isAim);
            animator.SetLayerWeight(1, 0); // 1번 레이어를 1로 활성화

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }


            if (isFirstPerson)
            {
                SetTargetFOV(defaultFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

    }

    void Fire()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //조준상태이고, 총을 쏘고있는 중이 아닌 때 사격
            if (isAim && !isFire && bulletCount > 0)
            {
                bulletCount--;
                Debug.Log("남은 총알 갯수 : " + bulletCount);

                //Weapon Type에 따라서 MaxDistance Set하도록 수정해야 함
                weaponMaxDistance = 1000.0f;

                isFire = true;

                //쏘는 시간 딜레이(너무 연사하지 않게) -> 무기별로 딜레이 시간 데이터를 바꿔야 함
                StartCoroutine(FireWithDelay(rifleFireDelay));
                animator.SetTrigger("Fire");

                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward); //new Ray(시작위치(메인카메라의 위치), 방향(메인카메라의 앞쪽방향))
                RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, TargetLayerMask);

                //두개의 물체만 받아오기
                if(hits.Length > 0)
                {
                    int count = 0;
                    foreach (RaycastHit hit in hits)
                    {
                        if(count > 1)
                        {
                            break;
                            
                        }
                        Debug.Log("충돌 : " + hit.collider.name);
                        Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                        count++;
                    }

                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green);
                }

                

                //if (Physics.Raycast(ray, out hit, weaponMaxDistance, TargetLayerMask)) // 만약 사정거리내에 ray가 충돌한다면 hit에 충돌한 객체의 정보가 들어가고 Raycast()는 true를 반환함  Physics.Raycast(ray, out hit, weaponMaxDistance, 레이어마스크)도 있음
                //{
                //    Debug.Log("Hit : " + hit.collider.gameObject.name);
                //    Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f); // ray가 눈에 보이게 그려줌
                //    if (hit.collider.gameObject.CompareTag("Zombie"))
                //    {
                //        //체력 깎기(임시로 1로 지정)
                //        hit.collider.gameObject.GetComponent<ZombieManager>().Hp -= 1;
                //        Debug.Log("Zombie Hp : " + hit.collider.gameObject.GetComponent<ZombieManager>().Hp);
                //    }
                //    //hit.collider.gameObject.SetActive(false); //맞은 물체 사라지게
                //}
                //else
                //{
                //    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
                //}
            }

        }
        if (Input.GetMouseButtonUp(0))
        {
            
        }
    }

   

    void Run()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;

        }
        else
        {
            isRunning = false;
        }
    }

    void WeaponChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && isGetWeapon)
        {
            isUseWepon = true;
            animator.SetTrigger("isWeaponChange");
            RifleM4Obj.SetActive(true);

        }
    }

    void AnimationSet()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("isRunning", isRunning);
        moveSpeed = isRunning ? runSpeed : walkSpeed;
    }

    void GetItemOperate()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //e키 입력시 애니메이션 실행
            animator.SetTrigger("PickUp");

            
        }
    }

    //손에 닿을 때 아이템 획득 하기 : 애니메이션 이벤트, 코루틴, Invoke 등등
    //애니메이션 이벤트로 사용하려고 public으로 함수를 따로 만들었음
    public void GetItem()
    {
        
        //아이템 줍기 구현
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer); //중심좌표, 반지름, 방향, 회전기본세팅,거리, 충돌할대상레이어
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Weapon"))
            {
                isGetWeapon = true;
                weaponIconObj.SetActive(true);
                audioSource.PlayOneShot(audioClipPickUp);
            }
            hit.collider.gameObject.SetActive(false);
            Debug.Log("Item : " + hit.collider.name);
            
        }
    }

    void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    void Update()
    {

        MouseSet();
        CameraSet();
        PlayerMovement();
        AimSet();
        Fire();
        Run();
        WeaponChange();
        AnimationSet();


        GetItemOperate();


        //애니메이션의 speed 조절
        animator.speed = animationSpeed;

        //애니메이터의 0번째 레이어에 있는 애니메이션의 정보들을 가져온다
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        //현재 애니메이션의 이름이 currentAnimation이고 , 이 애니메이션의 (정규화된)시간이 1.0초 이상이면(해당 애니메이션이 끝났으면)
        if(stateInfo.IsName(currentAnimation) && stateInfo.normalizedTime >= 1.0f)
        {
            //현재 애니메이션을 "Attack"으로 설정
            currentAnimation = "Attack";
            //애니메이션을 실행
            animator.Play(currentAnimation);
        }
    }

    

    //여기 주석은 내 개인 해석
    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal; //카메라 방향으로 이동방향을 계산
        moveDirection.y = 0; //단 이동방향의 y 좌표는 0으로 고정(캐릭터는 상하로 이동하진 않을 것이므로. y축의 이동을 고정)
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime); //캐릭터를 해당 방향으로 정해진 속도만큼 이동
        cameraTransform.position = playerHead.position; //카메라의 위치를 플레이어의 머리쪽 위치와 같게 옮긴다
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0); //카메라의 회전을 지정

        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0); // 캐릭터의 회전을 카메라 y축회전만큼만 회전(캐릭터는 좌우만 움직일것이므로)
    }

    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        //카메라가 플레이어 주위를 회전하는 부분
        if(isRotaterAroundPlayer)
        {
            //카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
            
        }
        else
        {
            //플레이어의 시야에 따라서 회전하는 부분
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();
        }
       
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;

    }

    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }

    //3인칭 줌
    IEnumerator ZoomCamera(float targetDistance)
    {
        while(Mathf.Abs(currentDistance - targetDistance) > 0.01f) //현재 거리에서 목표 거리로 부드럽게 이동
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // 목표거리에 도달한 후 값을 고정
    }

    //1인칭 줌
    IEnumerator ZoomFieldOfView(float targetFov)
    {
        while(Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView = targetFov;
    }

    IEnumerator FireWithDelay(float fireDelay)
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
    }

    public void WeaponChangeSoundOn()
    {
        audioSource.PlayOneShot(audioClipWeaponChange);
        //예외처리
        //소리를 내라 -> 클립이 없는 경우
        //다른 변수 등을 부를 때 해당 하는게 없을 경우
    }

    public void FireSoundOn()
    {
        //총 쏘는 소리
        audioSource.PlayOneShot(audioClipFire);
        //이펙트 재생
        WeaponEffect.Play();
    }

    public void FootStepSoundOn()
    {
        //if(밟은게 무엇이냐에 따라서)
        //{audioSource.PlayOneShot(발자국소리);}

        //raycast를 사용해서 밟은게 무엇인지 판단하는 방법
        //if(Physics.Raycast(transform.position, transform.forward, out hit, 10.0f, layerMask))
        //{

        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("PlayerDamage"))
        {
            //소리재생
            audioSource.PlayOneShot(audioClipFire);
            //애니메이션재생
            
            animator.SetTrigger("Damage");
            //처음위치로 이동
            characterController.enabled = false;
            transform.position = Vector3.zero;
            characterController.enabled = true;
            //충돌한 대상의 태그를 바꾸는 코드(사용예 : 아군-> 적군 바뀜, 갑옷 -> 깨지면 데미지, 주의점 : tag에 대한 설계 주의(예외처리 잘해야함))
            other.gameObject.tag = "Zombie";
        }
        
    }

    

   

}
