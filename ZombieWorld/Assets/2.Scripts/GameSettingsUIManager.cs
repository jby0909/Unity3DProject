using UnityEngine;
using UnityEngine.UI;

public class GameSettingsUIManager : MonoBehaviour
{
    public GameObject SettingsObj;

    public Text resolutionText;
    public Text graphicsQualityText;
    public Text fullScreenText;

    private int resolutionIndex;
    private int qualityIndex;
    private bool isFullScreen = true;

    private string[] resolutions = { "1280 × 720", "1920 × 1080", "2560 × 1440", "3840 × 2160" };
    private string[] qualityOptions = { "Low", "Normal", "High" };
    
    void Start()
    {
    }

    public void OnResolutionLeftClick()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionText();
    }

    public void OnResolutionRightClick()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionText();
    }

    public void OnGraphicsLeftClick()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
        UpdateGraphicsQualityText();
    }

    public void OnGraphicsRightClick()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
        UpdateGraphicsQualityText();
    }

    public void OnFullScreenToggleClick()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        isFullScreen = !isFullScreen;
        UpdateFullScreenText();
    }

    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
        UpdateFullScreenText();
    }

    private void UpdateGraphicsQualityText()
    {
        graphicsQualityText.text = qualityOptions[qualityIndex];
    }

    private void UpdateFullScreenText()
    {
        fullScreenText.text = isFullScreen ? "On" : "Off";
    }

    public void OnApplySettingsClick()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        ApplySettings();
        SaveSettings();
    }
    private void ApplySettings()
    {
        
        string[] res = resolutions[resolutionIndex].Split('×');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", qualityIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();

    }
    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1); //만약 해당 값이 없으면 1을 디폴트값으로 넣음
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);
    }
    public void OnSettings()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        SettingsObj.SetActive(true);
    }

    public void ExitSettings()
    {
        SoundManager.Instance.PlaySfx("UIClick", transform.position);
        SettingsObj.SetActive(false);
    }
}
