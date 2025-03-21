using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;

    private float maxAngle = 120.0f;
    private float currentAngle = 0;

    void Start()
    {
        
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
