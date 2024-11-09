using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour
{
    [SerializeField] private GameObject messageWindowObject;
    [SerializeField] private Image messageWindowIcon;
    [SerializeField] private TextMeshProUGUI messageWindowTitleText;
    [SerializeField] private TextMeshProUGUI messageWindowMessageText;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color warningColor;
    [SerializeField] private Color ErrorColor;

    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite errorSprite;

    [SerializeField] private Button messageButton;

    public delegate void MessageWindowDelegate();

    // Start is called before the first frame update
    void Start()
    {
        messageWindowObject.SetActive(false);
    }

    //Normal Message
    public void ShowMessage(string _title, string _message, Sprite _icon = null, MessageWindowDelegate _messageButtonDelegate = null, bool _showButton = false)
    {
        messageWindowObject.SetActive(true);
        messageWindowTitleText.text = _title;
        messageWindowMessageText.text = _message;
        messageWindowIcon.color = normalColor;

        if (_icon != null)
        {
            messageWindowIcon.enabled = true;
            messageWindowIcon.sprite = _icon;
        }
        else
        {
            messageWindowIcon.enabled = false;
        }

        messageButton.onClick.RemoveAllListeners();

        if(_messageButtonDelegate != null)
            messageButton.onClick.AddListener(() => _messageButtonDelegate());

        messageButton.gameObject.SetActive(_showButton);
    }

    //Warning Message
    public void ShowWarningMessage(string _title, string _message, Sprite _icon = null, MessageWindowDelegate _messageButtonDelegate = null, bool _showButton = true)
    {
        messageWindowObject.SetActive(true);
        messageWindowTitleText.text = _title;
        messageWindowMessageText.text = _message;
        messageWindowIcon.color = warningColor;

        messageButton.onClick.RemoveAllListeners();

        if (_messageButtonDelegate != null)
            messageButton.onClick.AddListener(() => _messageButtonDelegate());

        messageButton.gameObject.SetActive(_showButton);


        if (_icon != null)
        {
            messageWindowIcon.enabled = true;
            messageWindowIcon.sprite = _icon;
        }
        else
        {
            messageWindowIcon.enabled = false;
        }
    }

    //Error Message
    public void ShowErrorMessage(string _title, string _message, Sprite _icon = null, MessageWindowDelegate _messageButtonDelegate = null, bool _showButton = true)
    {
        messageWindowObject.SetActive(true);
        messageWindowTitleText.text = _title;
        messageWindowMessageText.text = _message;
        messageWindowIcon.color = ErrorColor;

        messageButton.onClick.RemoveAllListeners();

        if (_messageButtonDelegate != null)
            messageButton.onClick.AddListener(() => _messageButtonDelegate());

        messageButton.gameObject.SetActive(_showButton);


        if (_icon != null)
        {
            messageWindowIcon.enabled = true;
            messageWindowIcon.sprite = _icon;
        }
        else
        {
            messageWindowIcon.enabled = true;
            messageWindowIcon.sprite = warningSprite;
        }
    }

    public void Hide()
    {
        messageWindowObject.SetActive(false);
    }
}
