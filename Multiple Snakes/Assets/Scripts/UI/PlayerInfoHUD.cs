using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoHUD : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    [SerializeField] GameObject playerHUDObject;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private TextMeshProUGUI playerKillCountText;
    [SerializeField] private GameObject[] eyes;
    [SerializeField] private GameObject[] deadEyes;

    [SerializeField] private SpriteRenderer powerupBackground;
    [SerializeField] private SpriteRenderer powerupIcon;

    public void SetPlayer(int _playerIndex)
    {
        playerData = MultiplayerManager.instance.GetPlayerDataFromIndex(_playerIndex);

        playerHUDObject.SetActive(true);

        UpdateHUD();
    }

    public void UpdateHUD()
    {
        foreach(SpriteRenderer spriteRenderer in spriteRenderers) 
        {
            spriteRenderer.color = MultiplayerManager.instance.GetColorFromIndex(playerData.GetPlayerColorIndex());
        }

        displayNameText.text = playerData.GetPlayerDisplayName().ToString();

        if(playerData.IsAlive())
        {
            foreach (GameObject eye in eyes)
            {
                eye.SetActive(true);
            }

            foreach (GameObject deadEyes in deadEyes)
            {
                deadEyes.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject eye in eyes)
            {
                eye.SetActive(false);
            }

            foreach (GameObject deadEyes in deadEyes)
            {
                deadEyes.SetActive(true);
            }

            powerupBackground.gameObject.SetActive(false);
        }

        if(MultiplayerManager.instance.GetGameSettings().GetGameMode().Equals(GameModes.SNEK_EXTERMINATION))
        {
            playerKillCountText.gameObject.SetActive(true);

            if (playerData.GetKillCount() == 0)
                playerKillCountText.text = $"";
            else if (playerData.GetKillCount() > 1)
                playerKillCountText.text = $"{playerData.GetKillCount()} Kills";
            else
                playerKillCountText.text = $"{playerData.GetKillCount()} Kill";
        }
        else
        {
            playerKillCountText.gameObject.SetActive(false);
        }

        PowerupDefinition currentPowerup = GameManager.instance.GetPlayerPowerup(playerData.GetClientID());
        if (currentPowerup != null)
        {
            if (!currentPowerup.Equals(Powerups.NONE))
            {
                currentPowerup = Powerups.GetPowerup(currentPowerup.GetID());

                powerupBackground.gameObject.SetActive(true);
                powerupBackground.color = currentPowerup.GetColor();
                powerupIcon.sprite = currentPowerup.GetIcon();
            }
            else
                powerupBackground.gameObject.SetActive(false);
        }
        else
            powerupBackground.gameObject.SetActive(false);
    }

    public void Reset()
    {
        playerHUDObject.SetActive(false);
        powerupBackground.gameObject.SetActive(false);
    }
}
