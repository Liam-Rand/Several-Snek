using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorToggle : MonoBehaviour
{
    [SerializeField] private int colorIndex = 0;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private GameObject unavailableImage;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            MultiplayerManager.instance.SetPlayerColor(colorIndex);
        });
    }

    private void Start()
    {
        MultiplayerManager.instance.OnPlayerDataNetworkListChanged += MultiplayerManager_OnPlayerDataNetworkListChanged;
    }

    private void MultiplayerManager_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    public void SetColor(int _colorIndex)
    {
        colorIndex = _colorIndex;
        backgroundImage.color = MultiplayerManager.instance.GetPlayerColors()[colorIndex];

        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if(MultiplayerManager.instance.GetLocalPlayerData().GetPlayerColorIndex() == colorIndex) 
        {
            selectedImage.SetActive(true);
            unavailableImage.SetActive(false);
        }
        else
        {
            selectedImage.SetActive(false);

            if (!MultiplayerManager.instance.IsColorAvailable(colorIndex)) 
                unavailableImage.SetActive(true);
            else
                unavailableImage.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        MultiplayerManager.instance.OnPlayerDataNetworkListChanged -= MultiplayerManager_OnPlayerDataNetworkListChanged;
    }
}
