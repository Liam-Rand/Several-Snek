using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMusicOnLoad : MonoBehaviour
{
    [SerializeField] AudioClip musicClip;

    private void Start()
    {
        if(AudioManager.instance.GetCurrentMusic() != musicClip)
            AudioManager.instance.SetMusic(musicClip);
    }
}
