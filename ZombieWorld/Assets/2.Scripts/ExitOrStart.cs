using UnityEngine;
using UnityEngine.Audio;

public class ExitOrStart : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audipClipUI;
    public GameObject UICanvas;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Exit()
    {
        audioSource.PlayOneShot(audipClipUI);
        Time.timeScale = 1;
        UICanvas.SetActive(false);
        Application.Quit();

        
    }

    public void StartGame()
    {
        audioSource.PlayOneShot(audipClipUI);
        Time.timeScale = 1;
        UICanvas.SetActive(false);
    }
}
