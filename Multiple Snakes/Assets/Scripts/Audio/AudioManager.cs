using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of AudioManager found!");
            return;
        }

        instance = this;

        DontDestroyOnLoad(instance.gameObject);
    }
    #endregion

    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource soundEffectsAudioSource;

    public AudioSource GetUIAudioSource() { return uiAudioSource; }
    public AudioSource GetMusicAudioSource() { return musicAudioSource; }
    public AudioSource GetSoundEffectsAudioSource() { return soundEffectsAudioSource; }

    public void SetMusic(AudioClip _audioClip)
    {
        musicAudioSource.Stop();
        musicAudioSource.clip = _audioClip;
        musicAudioSource.Play();
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
    }

    public AudioClip GetCurrentMusic() { return musicAudioSource.clip; }
}
