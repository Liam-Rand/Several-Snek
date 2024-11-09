using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameVersionText;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject devWarningPanel;

    private void Start()
    {
        gameVersionText.text = $"Created by Coba Platinum\nMusic by Kali Rand\n{Application.version}";

        if (SettingsManager.instance.GetShowDevWarning())
        {
            devWarningPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
        }
        else
        {
            devWarningPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_Lobby", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleShowDevWarning(bool _showDevWarning)
    {
        SettingsManager.instance.ToggleShowDevWarning(_showDevWarning);
    }

    public void ReportBug()
    {
        Application.OpenURL("https://bug.cobaplatinum.com");
    }

    public void WatchOnYoutube()
    {
        Application.OpenURL("https://youtu.be/h5N_ieUOlr8");
    }
}
