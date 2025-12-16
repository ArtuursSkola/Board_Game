using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Main pause panel (Panelsss)")]
    public GameObject pausePanel;
    [Tooltip("Settings sub-panel")]
    public GameObject settingsPanel;

    [Header("Buttons")]
    [Tooltip("Button that opens the pause menu (optional wire-up if using UI events)")]
    public Button pauseButton;
    [Tooltip("Button to resume from pause")]
    public Button resumeButton;
    [Tooltip("Button to go to home scene")]
    public Button exitButton;
    [Tooltip("Button to open settings from pause panel")]
    public Button openSettingsButton;
    [Tooltip("Button to close settings back to pause panel")]
    public Button closeSettingsButton;
    [Tooltip("Scene name to load when exiting to home/menu")]
    public string homeSceneName = "MainMenu";

    [Header("Audio")]
    [Tooltip("Music AudioSource to control volume")]
    public AudioSource musicSource;
    [Tooltip("Optional list of SFX AudioSources to control volume")]
    public List<AudioSource> sfxSources = new List<AudioSource>();
    public Slider musicSlider;
    public Slider sfxSlider;
    public string musicVolumePrefKey = "MusicVolume"; // shared with other scene
    public string sfxVolumePrefKey = "SfxVolume";

    private float cachedTimeScale = 1f;

    void Start()
    {
        // Ensure panels start hidden
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Wire buttons if assigned
        if (pauseButton != null) pauseButton.onClick.AddListener(ShowPause);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ExitToHome);
        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);

        // Load volumes from prefs
        float musicVol = PlayerPrefs.GetFloat(musicVolumePrefKey, musicSource != null ? musicSource.volume : 1f);
        float sfxVol = PlayerPrefs.GetFloat(sfxVolumePrefKey, 1f);

        ApplyMusicVolume(musicVol);
        ApplySfxVolume(sfxVol);

        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    // Called by the Settings button to open the pause menu
    public void ShowPause()
    {
        cachedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = cachedTimeScale == 0f ? 1f : cachedTimeScale;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ExitToHome()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
        }
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value);
        PlayerPrefs.SetFloat(musicVolumePrefKey, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        ApplySfxVolume(value);
        PlayerPrefs.SetFloat(sfxVolumePrefKey, value);
    }

    private void ApplyMusicVolume(float value)
    {
        if (musicSource != null) musicSource.volume = value;
    }

    private void ApplySfxVolume(float value)
    {
        if (sfxSources == null) return;
        foreach (var src in sfxSources)
        {
            if (src != null) src.volume = value;
        }
    }
}
