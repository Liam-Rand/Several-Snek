using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    #region Singleton
    public static SettingsManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of SettingsManager found!");
            return;
        }

        instance = this;

        DontDestroyOnLoad(instance.gameObject);
    }
    #endregion

    [Header("Video Settings")]
    [SerializeField] private string fullscreenParameter = "Fullscreen";
    [SerializeField] private string vsyncPrameter = "Vsync";
    [SerializeField] private string resolutionParameter = "Resolution";
    [SerializeField] private List<ResolutionStruct> resolutions = new List<ResolutionStruct>();
    [SerializeField] private int defaultResolutionIndex;
    [SerializeField] private int currentResolutionIndex;

    [Space]
    [SerializeField] private bool toggleFullscreen;
    [SerializeField] private bool toggleVsync;

    [Header("Sound Settings")]
    [SerializeField] private float defaultAudioLevel = 0f;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string soundEffectsVolumeParameter = "SoundEffectsVolume";
    [SerializeField] private string uiVolumeParameter = "UIVolume";
    [SerializeField] private AudioMixer audioMixer;

    [Space]
    [SerializeField] private float masterVolume;
    [SerializeField] private float musicVolume;
    [SerializeField] private float soundEffectsVolume;
    [SerializeField] private float uiVolume;

    [Header("Control Settings")]
    [SerializeField] private InputActionAsset inputActionAsset;

    [Header("Background Settings")]
    [SerializeField] private bool showDevWarning;

    private void Start()
    {
        toggleFullscreen = (PlayerPrefs.GetInt(fullscreenParameter, Screen.fullScreen ? 1 : 0) == 1) ? true : false;
        toggleVsync = (PlayerPrefs.GetInt(vsyncPrameter, (QualitySettings.vSyncCount != 0) ? 1 : 0) == 1) ? true : false;

        currentResolutionIndex = PlayerPrefs.GetInt(resolutionParameter, defaultResolutionIndex);

        masterVolume = PlayerPrefs.GetFloat(masterVolumeParameter, defaultAudioLevel);
        musicVolume = PlayerPrefs.GetFloat(musicVolumeParameter, defaultAudioLevel);
        soundEffectsVolume = PlayerPrefs.GetFloat(soundEffectsVolumeParameter, defaultAudioLevel);
        uiVolume = PlayerPrefs.GetFloat(uiVolumeParameter, defaultAudioLevel);

        SetToggleFullscreen(toggleFullscreen);
        SetToggleVsync(toggleVsync);

        SetResolution(currentResolutionIndex);

        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSoundEffectsVolume(soundEffectsVolume);
        SetUIVolume(uiVolume);
    }

    public void SetToggleFullscreen(bool _toggleFullscreen)
    {
        toggleFullscreen = _toggleFullscreen;
        Screen.fullScreenMode = toggleFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt(fullscreenParameter, toggleFullscreen ? 1 : 0);
    }
    public void SetToggleVsync(bool _toggleVsync)
    {
        toggleVsync = _toggleVsync;
        QualitySettings.vSyncCount = toggleVsync ? 1 : 0;
        PlayerPrefs.SetInt(vsyncPrameter, toggleVsync ? 1 : 0);
    }
    public void SetResolution(int _resolutionIndex)
    {
        currentResolutionIndex = _resolutionIndex;
        Screen.SetResolution(resolutions[_resolutionIndex].GetHorizontal(), resolutions[_resolutionIndex].GetVertical(), toggleFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        PlayerPrefs.SetInt(resolutionParameter, currentResolutionIndex);
    }

    public void ResetAllBindings()
    {
        foreach (InputActionMap inputActionMap in inputActionAsset.actionMaps)
        {
            inputActionMap.RemoveAllBindingOverrides();
        }

        PlayerPrefs.DeleteKey("rebinds");
    }

    public void SetMasterVolume(float _masterVolume)
    {
        masterVolume = _masterVolume;
        audioMixer.SetFloat(masterVolumeParameter, masterVolume);
        PlayerPrefs.SetFloat(masterVolumeParameter, masterVolume);
    }
    public void SetMusicVolume(float _musicVolume)
    {
        musicVolume = _musicVolume;
        audioMixer.SetFloat(musicVolumeParameter, musicVolume);
        PlayerPrefs.SetFloat(musicVolumeParameter, musicVolume);
    }
    public void SetSoundEffectsVolume(float _soundEffectsVolume)
    {
        soundEffectsVolume = _soundEffectsVolume;
        audioMixer.SetFloat(soundEffectsVolumeParameter, soundEffectsVolume);
        PlayerPrefs.SetFloat(soundEffectsVolumeParameter, soundEffectsVolume);
    }
    public void SetUIVolume(float _uiVolume)
    {
        uiVolume = _uiVolume;
        audioMixer.SetFloat(uiVolumeParameter, uiVolume);
        PlayerPrefs.SetFloat(uiVolumeParameter, uiVolume);
    }
    public void ToggleShowDevWarning(bool _showDevWarning) { showDevWarning = _showDevWarning; }

    public bool GetToggleFullscreen() { return toggleFullscreen; }
    public bool GetToggleVsync() { return toggleVsync; }
    public List<ResolutionStruct> GetResolutions() { return resolutions; }
    public float GetMasterVolume() { return masterVolume; }
    public float GetMusicVolume() { return musicVolume; }
    public float GetSoundEffectsVolume() { return soundEffectsVolume; }
    public float GetUIVolume() { return uiVolume; }
    public float GetDefaultAudioLevel() { return defaultAudioLevel; }
    public int GetDefaultResolutionIndex() { return defaultResolutionIndex; }
    public int GetCurrentResolutionIndex() { return currentResolutionIndex; }
    public InputActionAsset GetPlayerInputActions() {  return inputActionAsset; }
    public bool GetShowDevWarning() { return showDevWarning; }
}

[System.Serializable]
public struct ResolutionStruct
{
    [SerializeField] private int horizontal, vertical;

    public int GetHorizontal() { return horizontal; }
    public int GetVertical() { return vertical; }

    public override string ToString()
    {
        return $"{horizontal} X {vertical}";
    }
}
