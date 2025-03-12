using Unity.VisualScripting;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
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
