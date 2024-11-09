using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    private SettingsManager settingsManager;

    [Header("Category Panels")]
    [SerializeField] private GameObject videoSettingsPanel;
    [SerializeField] private GameObject graphicsSettingsPanel;
    [SerializeField] private GameObject audioSettingsPanel;
    [SerializeField] private GameObject controlsSettingsPanel;

    [Header("Video Settings")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private ButtonListSelectUI resolutionSelect;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectsVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;

    [SerializeField] private Toggle masterVolumeToggle;
    [SerializeField] private Toggle musicVolumeToggle;
    [SerializeField] private Toggle soundEffectsVolumeToggle;
    [SerializeField] private Toggle uiVolumeToggle;

    private bool disableToggleEvent = false;

    private void Awake()
    {
        settingsManager = SettingsManager.instance;
    }

    private void UpdateUI()
    {
        fullscreenToggle.isOn = settingsManager.GetToggleFullscreen();
        vsyncToggle.isOn = settingsManager.GetToggleVsync();

        List<string> resolutionStrings = new List<string>();
        for(int i = 0; i < settingsManager.GetResolutions().Count; i++)
        {
            resolutionStrings.Add(settingsManager.GetResolutions()[i].ToString());
        }

        resolutionSelect.SetDefaultOption(settingsManager.GetCurrentResolutionIndex());
        resolutionSelect.SetOptions(resolutionStrings);

        masterVolumeSlider.value = settingsManager.GetMasterVolume();
        musicVolumeSlider.value = settingsManager.GetMusicVolume();
        soundEffectsVolumeSlider.value = settingsManager.GetSoundEffectsVolume();
        uiVolumeSlider.value = settingsManager.GetUIVolume();
    }

    public void SetToggleFullscreen(bool _toggleFullscreen)
    {
        settingsManager.SetToggleFullscreen(_toggleFullscreen);
    }
    public void SetToggleVsync(bool _toggleVsync)
    {
        settingsManager.SetToggleVsync(_toggleVsync);
    }
    public void SetResolution(int _resolution)
    {
        settingsManager.SetResolution(_resolution);
    }

    public void SetMasterVolume(float _masterVolume)
    {
        settingsManager.SetMasterVolume(_masterVolume);

        disableToggleEvent = true;
        masterVolumeToggle.isOn = _masterVolume > masterVolumeSlider.minValue;
        disableToggleEvent = false;
    }
    public void ToggleMasterVolume(bool _toggle)
    {
        if (disableToggleEvent) return;

        if (_toggle)
            masterVolumeSlider.value = settingsManager.GetDefaultAudioLevel();
        else
            masterVolumeSlider.value = masterVolumeSlider.minValue;
    }

    public void SetMusicVolume(float _masterVolume)
    {
        settingsManager.SetMusicVolume(_masterVolume);

        disableToggleEvent = true;
        musicVolumeToggle.isOn = _masterVolume > musicVolumeSlider.minValue;
        disableToggleEvent = false;
    }
    public void ToggleMusicVolume(bool _toggle)
    {
        if (disableToggleEvent) return;

        if (_toggle)
            musicVolumeSlider.value = settingsManager.GetDefaultAudioLevel();
        else
            musicVolumeSlider.value = musicVolumeSlider.minValue;
    }

    public void SetSoundEffectsVolume(float _masterVolume)
    {
        settingsManager.SetSoundEffectsVolume(_masterVolume);

        disableToggleEvent = true;
        soundEffectsVolumeToggle.isOn = _masterVolume > soundEffectsVolumeSlider.minValue;
        disableToggleEvent = false;
    }
    public void ToggleSoundEffectsVolume(bool _toggle)
    {
        if (disableToggleEvent) return;

        if (_toggle)
            soundEffectsVolumeSlider.value = settingsManager.GetDefaultAudioLevel();
        else
            soundEffectsVolumeSlider.value = soundEffectsVolumeSlider.minValue;
    }

    public void SetUIVolume(float _masterVolume)
    {
        settingsManager.SetUIVolume(_masterVolume);

        disableToggleEvent = true;
        uiVolumeToggle.isOn = _masterVolume > uiVolumeSlider.minValue;
        disableToggleEvent = false;
    }
    public void ToggleUIVolume(bool _toggle)
    {
        if (disableToggleEvent) return;

        if (_toggle)
            uiVolumeSlider.value = settingsManager.GetDefaultAudioLevel();
        else
            uiVolumeSlider.value = uiVolumeSlider.minValue;
    }

    public void ShowVideoSettings()
    {
        videoSettingsPanel.SetActive(true);
        graphicsSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(false);
    }
    public void ShowGraphicsSettings()
    {
        videoSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(true);
        audioSettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(false);
    }
    public void ShowAudioSettings()
    {
        videoSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(true);
        controlsSettingsPanel.SetActive(false);
    }
    public void ShowControlsSettings()
    {
        videoSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(true);
    }

    public void ResetAllBindings()
    {
        settingsManager.ResetAllBindings();
    }

    private void OnEnable()
    {
        UpdateUI();
    }
}
