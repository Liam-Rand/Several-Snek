using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle;

    [SerializeField] private Image backgroundImage;

    [SerializeField] private Image readyImage;
    [SerializeField] private Image notReadyImage;

    [SerializeField] private Color readyColor;
    [SerializeField] private Color notReadyColor;

    private void Start()
    {
        Toggle(toggle.isOn);
    }

    public void Toggle(bool _toggle)
    {
        if (_toggle) 
        {
            backgroundImage.color = readyColor;
            readyImage.enabled = true;
            notReadyImage.enabled = false;
        }
        else
        {
            backgroundImage.color = notReadyColor;
            readyImage.enabled = false;
            notReadyImage.enabled = true;
        }
    }
}
