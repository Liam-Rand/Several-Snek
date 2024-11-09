using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [Header("Snake Screen")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Snake Preview")]
    [SerializeField] private Image[] snakePreviewImages;
    [SerializeField] private TextMeshProUGUI displayNameText;

    [Header("Continue Button")]
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject waitingForHostText;

    private ulong winningPlayerID;

    public void SetWinningSnake(ulong _winningPlayerID)
    {
        winningPlayerID = _winningPlayerID;
        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach(Image snakePreviewImage in snakePreviewImages) 
        {
            snakePreviewImage.color = MultiplayerManager.instance.GetColorFromIndex(
                MultiplayerManager.instance.GetPlayerDataFromClientId(winningPlayerID).GetPlayerColorIndex());
        }

        displayNameText.text = MultiplayerManager.instance.GetPlayerDataFromClientId(winningPlayerID).GetPlayerDisplayName().ToString();

        if (NetworkManager.Singleton.IsServer)
        {
            continueButton.SetActive(true);
            waitingForHostText.SetActive(false);
        }
        else
        {
            continueButton.SetActive(false);
            waitingForHostText.SetActive(true);
        }
    }

    public void Show(){ gameOverPanel.SetActive(true); }
    public void Hide() { gameOverPanel.SetActive(false); }
}
