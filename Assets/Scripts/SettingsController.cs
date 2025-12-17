using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource musicSource;
    public Slider musicSlider;

    [Header("Music Selection")]
    public Dropdown musicDropdown;               // NEW: Dropdown for music selection
    public List<AudioClip> musicTracks;          // NEW: List of music tracks to choose from

    [Header("Resolution")]
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    Resolution[] availableResolutions;

    const string PREF_MUSIC_VOLUME = "MusicVolume";
    const string PREF_RES_INDEX = "ResolutionIndex";
    const string PREF_FULLSCREEN = "Fullscreen";
    const string PREF_MUSIC_INDEX = "MusicIndex";    // NEW: Save selected music track

    void Start()
    {
        InitMusicDropdown();
        InitResolutionDropdown();
        InitFullscreen();
        InitVolume();
    }

    // -------------------- MUSIC SELECTION ----------------------
    void InitMusicDropdown()
    {
        if (musicDropdown == null || musicTracks == null) return;

        // Build list of music names
        List<string> names = new List<string>();
        foreach (AudioClip clip in musicTracks)
        {
            names.Add(clip != null ? clip.name : "Unnamed Track");
        }

        musicDropdown.ClearOptions();
        musicDropdown.AddOptions(names);

        int savedIndex = PlayerPrefs.GetInt(PREF_MUSIC_INDEX, 0);
        savedIndex = Mathf.Clamp(savedIndex, 0, musicTracks.Count - 1);

        musicDropdown.value = savedIndex;
        musicDropdown.RefreshShownValue();

        musicDropdown.onValueChanged.AddListener(OnMusicTrackChanged);

        // Play saved/selected track
        PlayMusicTrack(savedIndex);
    }

    public void OnMusicTrackChanged(int index)
    {
        PlayMusicTrack(index);
        PlayerPrefs.SetInt(PREF_MUSIC_INDEX, index);
    }

    void PlayMusicTrack(int index)
    {
        if (musicSource == null || musicTracks == null || index >= musicTracks.Count)
            return;

        musicSource.clip = musicTracks[index];
        musicSource.Play();
    }

    // -------------------- VOLUME ----------------------
    void InitVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, musicSource != null ? musicSource.volume : 1f);

        if (musicSlider != null)
        {
            musicSlider.value = savedVolume;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (musicSource != null) musicSource.volume = savedVolume;
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (musicSource != null) musicSource.volume = value;
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, value);
    }

    // -------------------- RESOLUTION ----------------------
    void InitResolutionDropdown()
    {
        availableResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = FormatResolution(availableResolutions[i]);

            if (!options.Contains(option))
                options.Add(option);

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);

            int savedIndex = PlayerPrefs.GetInt(PREF_RES_INDEX, currentResolutionIndex);
            savedIndex = Mathf.Clamp(savedIndex, 0, options.Count - 1);
            resolutionDropdown.value = savedIndex;
            resolutionDropdown.RefreshShownValue();

            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
    }

    public void OnResolutionChanged(int dropdownIndex)
    {
        if (availableResolutions == null || availableResolutions.Length == 0) return;

        List<int> mapping = new List<int>();
        List<string> seen = new List<string>();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = FormatResolution(availableResolutions[i]);
            if (!seen.Contains(option))
            {
                seen.Add(option);
                mapping.Add(i);
            }
        }

        dropdownIndex = Mathf.Clamp(dropdownIndex, 0, mapping.Count - 1);
        Resolution res = availableResolutions[mapping[dropdownIndex]];
        bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;

        FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(res.width, res.height, mode, res.refreshRateRatio);
        PlayerPrefs.SetInt(PREF_RES_INDEX, dropdownIndex);
    }

    void InitFullscreen()
    {
        bool isFullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }
    }

    public void OnFullscreenToggleChanged(bool isFullscreen)
    {
        int dropdownIndex = resolutionDropdown != null ? resolutionDropdown.value : 0;
        OnResolutionChanged(dropdownIndex);
        PlayerPrefs.SetInt(PREF_FULLSCREEN, isFullscreen ? 1 : 0);
    }

    // -------------------- PANELS ----------------------
    public GameObject settingsPanel;
    public GameObject buttonsPanel;

    public void ShowSettings()
    {
        if (buttonsPanel != null) buttonsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (buttonsPanel != null) buttonsPanel.SetActive(true);
    }

    private string FormatResolution(Resolution res)
    {
        float hz = (float)res.refreshRateRatio.value;
        return res.width + " x " + res.height + " @ " + Mathf.RoundToInt(hz) + "Hz";
    }
}
