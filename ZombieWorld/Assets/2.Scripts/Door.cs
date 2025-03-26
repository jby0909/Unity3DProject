using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;
    private Animator animator;
    
    //마지막으로 문이 정방향으로 열려있는지를 추적
    public bool LastOpenForward { get; private set; } = true; 

    private float maxAngle = 120.0f;
    private float currentAngle = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("Player"))
        //{
        //    StartCoroutine(OpenDoor());
        //}
    }

    public bool IsPlayerInFront(Transform player)
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        //플레이어와 문 사이의 벡터를 계산
        float dotProduct = Vector3.Dot(transform.forward, toPlayer);
        //문이 향하는 방향과 플레이어의 방향을 비교(내적연산)
        return dotProduct > 0; 
        //dotProduct > 0 이면 플레이어가 문 앞에 있음
    }

    public bool Open(Transform player)
    {
        if(!isOpen) 
        {
            isOpen = true; // 문이 열린 상태로 설정

            if (IsPlayerInFront(player)) //플레이어가 앞에 있으면 정방향 애니 재생, 뒤에 있으면 역방향 애니 재생
            {
                animator.SetTrigger("OpenForward"); //정방향
                LastOpenForward = true; //문이 정방향으로 열림
            }
            else
            {
                animator.SetTrigger("OpenBackward"); // 역방향
                LastOpenForward = false; //문이 역방향으로 열림
            }
            return true;
        }
        

        return false;
    }

    public void CloseForward(Transform player)
    {
        if(isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseForward");
        }
    }

    public void CloseBackward(Transform player)
    {
        if(isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseBackward");
        }
    }

    
    IEnumerator OpenDoor( )
    {
        while(currentAngle <= maxAngle)
        {
            transform.GetChild(0).transform.rotation = Quaternion.Euler(-90f,0f, currentAngle);
            currentAngle += Time.deltaTime * 2.0f;
            yield return null;
        }
    }
}
