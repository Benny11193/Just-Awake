using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class MainMenuManage : MonoBehaviour
{
    public TMP_Dropdown ResolutionDropdown;
    public Toggle FullScreenToggle;
    public TMP_Dropdown GraphicDropdown;
    public Slider MusicValueSlider;
    public Slider SoundEffectValueSlider;
    public Animator animator;
    public AudioMixer audioMixer;


    Resolution[] resolutions;

    void Awake()
    {
        bool fullscreenStatus = (PlayerPrefs.GetInt("FullScreen", 1) == 1) ? true : false;
        Screen.SetResolution(PlayerPrefs.GetInt("ResolutionWidth", 1920), PlayerPrefs.GetInt("ResolutionHeight", 1080), fullscreenStatus);

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphic", 2)); // '2' is the "High" level

        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 0));
        audioMixer.SetFloat("SoundEffectVolume", PlayerPrefs.GetFloat("SoundEffectVolume", 0));
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        animator.Play("BlackBoardFadeOut");

        resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = currentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();

        FullScreenToggle.isOn = Screen.fullScreen;

        GraphicDropdown.value = QualitySettings.GetQualityLevel();
        GraphicDropdown.RefreshShownValue();

        float musicVolLevel;
        bool musicVolLevelResult = audioMixer.GetFloat("MusicVolume", out musicVolLevel);
        if (musicVolLevelResult) MusicValueSlider.value = musicVolLevel;

        float soundEffectVolLevel;
        bool soundEffectVolLevelResult = audioMixer.GetFloat("SoundEffectVolume", out soundEffectVolLevel);
        if (soundEffectVolLevelResult) SoundEffectValueSlider.value = soundEffectVolLevel;
    }

    public void PlayButton()
    {
        animator.Play("BlackBoardFadeIn");
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void SetMusicVolume (float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SetSoundEffectVolume (float volume)
    {
        audioMixer.SetFloat("SoundEffectVolume", volume);
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen (bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution (int resolutionindex)
    {
        Resolution resolution = resolutions[resolutionindex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SaveOptionPref()
    {
        PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);

        int fullscreenStatus = Screen.fullScreen ? 1 : 0;
        PlayerPrefs.SetInt("FullScreen", fullscreenStatus);

        PlayerPrefs.SetInt("Graphic", QualitySettings.GetQualityLevel());

        float musicVolLevel;
        bool musicVolLevelResult = audioMixer.GetFloat("MusicVolume", out musicVolLevel);
        if (musicVolLevelResult) PlayerPrefs.SetFloat("MusicVolume", musicVolLevel);

        float soundEffectVolLevel;
        bool soundEffectVolLevelResult = audioMixer.GetFloat("SoundEffectVolume", out soundEffectVolLevel);
        if (soundEffectVolLevelResult) PlayerPrefs.SetFloat("SoundEffectVolume", soundEffectVolLevel);
    }
}
