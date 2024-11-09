using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] private Animator animator;

    void Awake()
    {
        if(animator == null)
            animator = GetComponent<Animator>();

        if(animator != null )
            animator.keepAnimatorStateOnDisable = true;
    }

    private void OnDisable()
    {
        if (animator != null)
            animator.SetTrigger("normal");
    }

    public void PlayAudioClip(AudioClip _audioClip)
    {
        AudioManager.instance.GetUIAudioSource().PlayOneShot(_audioClip);
    }
}
