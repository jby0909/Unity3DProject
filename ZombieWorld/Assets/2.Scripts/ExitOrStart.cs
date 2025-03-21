using UnityEngine;
using UnityEngine.Audio;

public class ExitOrStart : MonoBehaviour
{
   
    public GameObject UICanvas;

    private void Start()
    {
        
    }

    public void Exit()
    {
        SoundManager.Instance.PlaySfx("UIClick");
        Time.timeScale = 1;
        UICanvas.SetActive(false);
        Application.Quit();
    }

    public void ReturnGame()
    {
        SoundManager.Instance.PlaySfx("UIClick");
        Time.timeScale = 1;
        UICanvas.SetActive(false);
    }
}
