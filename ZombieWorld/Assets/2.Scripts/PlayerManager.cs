using System;
using System.Collections;
using UnityEngine; // NameSpace : �Ҽ�


public class PlayerManager : MonoBehaviour
{
    private float moveSpeed = 5.0f; //�÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f; // ���콺 ����
    public Transform cameraTransform; // ī�޶��� Transform
    public CharacterController characterController;
    public Transform playerHead; //�÷��̾� �Ӹ� ��ġ(1��Ī ��带 ���ؼ�)
    public float thirdPersonDistance = 3.0f; // 3��Ī ��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); //3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; //�÷��̾� �þ� ��ġ

    public float zoomeDistance = 1.0f; //ī�޶� Ȯ��ɶ��� �Ÿ�(3��Ī ��忡�� ���)
    public float zoomSpeed = 5.0f; //Ȯ����Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60.0f; //�⺻ ī�޶� �þ߰�
    public float zoomeFov = 30.0f; //Ȯ�� �� ī�޶� �þ߰�(1��Ī ��忡�� ���)

    private float currentDistance; //���� ī�޶���� �Ÿ�(3��Ī ���)
    private float targetDistance; //��ǥ ī�޶� �Ÿ�
    private float targetFov; //��ǥ FOV
    private bool isZoomed = false; // Ȯ�� ���� Ȯ��
    private Coroutine zoomCoroutine; //�ڷ�ƾ�� ����Ͽ� Ȯ�� ��� ó��
    private Camera mainCamera; //ī�޶� ������Ʈ

    private float pitch = 0.0f; //���Ʒ� ȸ�� ��
    private float yaw = 0.0f; //�¿� ȸ����
    private bool isFirstPerson = false; //1��Ī ��� ����
    private bool isRotaterAroundPlayer = true; //ī�޶� �÷��̾� ������ ȸ���ϴ��� ����

    //�߷� ���� ����
    public float gravity = -9.81f; //CharacterController������ �߷��� ����ȵż� ���� �������ش�?
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround; //���� ��Ҵ��� ����

    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunning = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    private bool isAim = false;
    private bool isFire = false;

    public AudioClip audioClipFire;
    private AudioSource audioSource;
    public AudioClip audioClipWeaponChange;
    public GameObject RifleM4Obj;
    

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
    }

    void Update()
    {
        //���콺 �Է��� �޾� ī�޶� �÷��̾� ȸ�� ó��
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        //���� ����(3��Ī���ӿ��� ���� -45 ~ 45 �� ���� ����) 
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        isGround = characterController.isGrounded;
        

        if(isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        if(Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1��Ī ���" : "3��Ī ���");
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾��� �þ߿� ���� ȸ���մϴ�.");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }

        //���� �ؼ�
        if(Input.GetMouseButtonDown(1)) //���콺 ������ ��ư ������ ��
        {
            isAim = true;
            animator.SetBool("isAim", isAim);
            

            if (zoomCoroutine != null) // �ڷ�ƾ�� �̹� �۵����� ��
            {
                StopCoroutine(zoomCoroutine); //�ش� �ڷ�ƾ�� �����
            }

            if(isFirstPerson) //1��Ī �����̸� -> ī�޶� ��ü�� �ܱ��
            {
                SetTargetFOV(zoomeFov); //Ȯ��� �þ߰��� ��ǥ �þ߰����� ����
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov)); //���� ������ �ڷ�ƾ�� ����

            }
            else // 3��Ī �����̸� -> ī�޶��� ��ġ �̵�
            {
                SetTargetDistance(zoomeDistance); // Ȯ��� �Ÿ��� ��ǥ �Ÿ��� ����
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance)); //���� ������ �ڷ�ƾ�� ����
            }
        }

        if(Input.GetMouseButtonUp(1)) //���콺 ������ ��ư ���� ��
        {
            isAim = false;
            animator.SetBool("isAim", isAim);

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            
            if(isFirstPerson)
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

        if (Input.GetMouseButtonDown(0))
        {
            if(isAim)
            {
                isFire = true;
                animator.SetBool("isFire", isFire);
                audioSource.PlayOneShot(audioClipFire);
            }
            
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFire = false;
            animator.SetBool("isFire", isFire);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
           
        }
        else
        {
            isRunning = false;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(audioClipWeaponChange);
            animator.SetTrigger("isWeaponChange");
            RifleM4Obj.SetActive(true);

        }
        
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("isRunning", isRunning);
        moveSpeed = isRunning ? runSpeed : walkSpeed;
    }

    

    //���� �ּ��� �� ���� �ؼ�
    void FirstPersonMovement()
    {
        if(!isAim)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal; //ī�޶� �������� �̵������� ���
            moveDirection.y = 0; //�� �̵������� y ��ǥ�� 0���� ����(ĳ���ʹ� ���Ϸ� �̵����� ���� ���̹Ƿ�. y���� �̵��� ����)
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime); //ĳ���͸� �ش� �������� ������ �ӵ���ŭ �̵�
        }
        

       

        cameraTransform.position = playerHead.position; //ī�޶��� ��ġ�� �÷��̾��� �Ӹ��� ��ġ�� ���� �ű��
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0); //ī�޶��� ȸ���� ����

        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0); // ĳ������ ȸ���� ī�޶� y��ȸ����ŭ�� ȸ��(ĳ���ʹ� �¿츸 �����ϰ��̹Ƿ�)
    }

    void ThirdPersonMovement()
    {
        if (!isAim)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            characterController.Move(move * moveSpeed * Time.deltaTime);
        }
        

        

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        //ī�޶� �÷��̾� ������ ȸ���ϴ� �κ�
        if(isRotaterAroundPlayer)
        {
            //ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            //�÷��̾��� �þ߿� ���� ȸ���ϴ� �κ�
            transform.rotation = Quaternion.Euler(0f, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
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

    //3��Ī ��
    IEnumerator ZoomCamera(float targetDistance)
    {
        while(Mathf.Abs(currentDistance - targetDistance) > 0.01f) //���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� �̵�
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // ��ǥ�Ÿ��� ������ �� ���� ����
    }

    //1��Ī ��
    IEnumerator ZoomFieldOfView(float targetFov)
    {
        while(Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView = targetFov;
    }
}
