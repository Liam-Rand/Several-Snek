using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject placeholderImage;
    [SerializeField] private GameObject snakePreview;
    [SerializeField] private TextMeshProUGUI placeholderDisplayNameText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private GameObject readyToggle;
    [SerializeField] private ReadyIndicator readyIndicator;
    [SerializeField] private GameObject kickButton;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip playerConnectSound;
    [SerializeField] private AudioClip playerDisconnectSound;

    [SerializeField] private Image[] snakePreviewColoredImages;

    private int playerIndex = -1;

    public void SetOwnerPlayer(int _playerIndex)
    {
        placeholderImage.SetActive(false);
        snakePreview.SetActive(true);
        placeholderDisplayNameText.gameObject.SetActive(false);
        displayNameText.gameObject.SetActive(true);
        readyToggle.SetActive(true);
        readyIndicator.gameObject.SetActive(false);
        kickButton.SetActive(false);

        playerIndex = _playerIndex;
    }

    public void EnableKick()
    {
        kickButton.SetActive(true);
    }

    public void SetServerPlayer(int _playerIndex, bool _playSound)
    {
        placeholderImage.SetActive(false);
        snakePreview.SetActive(true);
        placeholderDisplayNameText.gameObject.SetActive(false);
        displayNameText.gameObject.SetActive(true);
        readyToggle.SetActive(false);
        readyIndicator.gameObject.SetActive(true);
        kickButton.SetActive(false);

        if(_playSound && playerIndex != _playerIndex)
            AudioManager.instance.GetUIAudioSource().PlayOneShot(playerConnectSound);

        playerIndex = _playerIndex;
    }

    public void Reset()
    {
        placeholderImage.SetActive(true);
        snakePreview.SetActive(false);
        placeholderDisplayNameText.gameObject.SetActive(true);
        displayNameText.gameObject.SetActive(false);
        readyToggle.SetActive(false);
        readyIndicator.gameObject.SetActive(false);
        kickButton.SetActive(false);

        if (playerIndex != -1)
            AudioManager.instance.GetUIAudioSource().PlayOneShot(playerDisconnectSound);
    }

    public void UpdateReadyState(bool _ready)
    {
        readyIndicator.Toggle(_ready);
    }

    public void UpdatePlayerColor(Color _color)
    {
        foreach (Image snakePreviewColoredImage in snakePreviewColoredImages)
        {
            snakePreviewColoredImage.color = _color;
        }
    }

    public void UpdatePlayerDisplayName(string _playerDisplayName)
    {
        displayNameText.text = _playerDisplayName;
    }

    public void KickPlayer()
    {
        PlayerData playerData = MultiplayerManager.instance.GetPlayerDataFromIndex(playerIndex);
        MultiplayerLobby.instance.KickPlayer(playerData.GetPlayerID().ToString());
        MultiplayerManager.instance.KickPlayer(playerData.GetClientID());
    }
}
