using CobaPlatinum.DebugTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Lobby Screen")]
    [SerializeField] private GameObject LobbyPanel;
    [SerializeField] private TMP_InputField playerDisplayNameInputField;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TextMeshProUGUI activeGamesCountText;

    [Header("Create Game Screen")]
    [SerializeField] private GameObject createGamePanel;
    [SerializeField] private TMP_InputField gameNameInputField;
    [SerializeField] private Toggle privateGameToggle;

    [Header("Message Screen")]
    [SerializeField] private MessageWindow messageWindow;
    [SerializeField] private Sprite connectingSprite;
    [SerializeField] private Sprite newGameSprite;

    [Header("Game Version")]
    [SerializeField] private string validGameVerisonURL;

    private void Start()
    {
        MultiplayerManager.instance.OnTryingToConnect += MultiplayerManager_OnTryingToConnect;
        MultiplayerManager.instance.OnFailedToConnect += MultiplayerManager_OnFailedToConnect;

        MultiplayerLobby.instance.OnCreateLobbyStarted += MultiplayerLobby_OnCreateLobbyStarted;
        MultiplayerLobby.instance.OnCreateLobbyFailed += MultiplayerLobby_OnCreateLobbyFailed;

        MultiplayerLobby.instance.OnJoinStarted += MultiplayerLobby_OnJoinStarted;
        MultiplayerLobby.instance.OnJoinFailed += MultiplayerLobby_OnJoinFailed;
        MultiplayerLobby.instance.OnQuickJoinFailed += MultiplayerLobby_OnQuickJoinFailed;

        MultiplayerLobby.instance.OnGamesCountChanged += MultiplayerLobby_OnGamesCountChanged;

        playerDisplayNameInputField.text = MultiplayerManager.instance.GetPlayerDisplayName();
    }

    private void MultiplayerLobby_OnGamesCountChanged(object sender, EventArgs e)
    {
        activeGamesCountText.text = $"Active Games: {MultiplayerLobby.instance.GetActiveGames()}/{MultiplayerLobby.instance.GetMaxActiveGames()}";
    }

    private void MultiplayerLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating New Game...", "", newGameSprite);
    }

    private void MultiplayerLobby_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowErrorMessage("Failed to Create Game!", "");
    }

    private void MultiplayerLobby_OnJoinStarted(object sender, EventArgs e)
    {
        ShowMessage("Joining Game...", "", connectingSprite);
    }

    private void MultiplayerLobby_OnJoinFailed(object sender, EventArgs e)
    {
        ShowErrorMessage("Failed to Join Game!", "");
    }

    private void MultiplayerLobby_OnQuickJoinFailed(object sender, EventArgs e)
    {
        ShowErrorMessage("Could Not Find A Public Game!", "");
    }

    public void PastJoinCode()
    {
        joinCodeInputField.text = GUIUtility.systemCopyBuffer;
    }

    private void ShowMessage(string _title, string _message, Sprite _icon)
    {
        LobbyPanel.SetActive(false);
        createGamePanel.SetActive(false);

        messageWindow.ShowMessage(_title, _message, _icon);
    }

    private void ShowErrorMessage(string _title, string _message)
    {
        LobbyPanel.SetActive(false);
        createGamePanel.SetActive(false);

        messageWindow.ShowErrorMessage(_title, _message, null, ReturnToLobby);
    }

    public void UpdatePlayerDisplayName(string _playerDisplayName)
    {
        MultiplayerManager.instance.SetPlayerDisplayName(_playerDisplayName);
    }

    private void MultiplayerManager_OnTryingToConnect(object sender, EventArgs e)
    {
        ShowMessage("Joining Game...", "", connectingSprite);
    }

    private void MultiplayerManager_OnFailedToConnect(object sender, EventArgs e)
    {
        ShowErrorMessage("Failed to Connect", NetworkManager.Singleton.DisconnectReason);
    }

    public void LeaveLobby()
    {
        MultiplayerLobby.instance.LeaveLobby();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_MainMenu", LoadSceneMode.Single);
    }

    public void CreateGame()
    {
        if (MultiplayerLobby.instance.AreActiveGamesAvailable())
        {
            if(IsValidGameVersion())
                MultiplayerLobby.instance.CreateLobby(gameNameInputField.text, !privateGameToggle.isOn);
            else
                ShowErrorMessage("Invalid Game Version!", "The version of the game you are on does not match\n" +
                    " the required version to join multiplayer games! \nPlease check to see if their is a new version available!");
        }
        else
            ShowErrorMessage("Failed to Create Game!", "The max number of active games has been reached!\n" +
                "Please wait for a game to end before creating a new one!");
    }

    public void QuickJoin()
    {
        if(IsValidGameVersion())
            MultiplayerLobby.instance.QuickJoin();
        else
            ShowErrorMessage("Invalid Game Version!", "The version of the game you are on does not match\n" +
                " the required version to join multiplayer games! \nPlease check to see if their is a new version available!");
    }

    public void JoinWithCode()
    {
        if (IsValidGameVersion())
            MultiplayerLobby.instance.JoinWithCode(joinCodeInputField.text);
        else
            ShowErrorMessage("Invalid Game Version!", "The version of the game you are on does not match\n" +
                " the required version to join multiplayer games! \nPlease check to see if their is a new version available!");
    }

    public bool IsValidGameVersion()
    {
//#if UNITY_EDITOR || DEVELOPMENT_BUILD
        //return true;
//#else
        string validGameVersion = DownloadTextFromURL(validGameVerisonURL);
        Match validGameVersionRegex = Regex.Match(validGameVersion, "\\(.*\\)");
        Match installedGameVersionRegex = Regex.Match(Application.version, "\\(.*\\)");
        CP_DebugWindow.Log(this, $"Valid Game Version: {validGameVersionRegex} | Current Game Version: {installedGameVersionRegex}");
        return validGameVersionRegex.ToString().Equals(installedGameVersionRegex.ToString());
//#endif
    }

    public string DownloadTextFromURL(string _address)
    {
        string text;
        WebClient client = new WebClient();
        Stream stream = client.OpenRead(_address);
        StreamReader reader = new StreamReader(stream);
        text = reader.ReadToEnd();
        reader.Close();
        stream.Close();
        client.Dispose();

        return text;
    }

    public void StartHost()
    {
        LobbyPanel.SetActive(false);
        MultiplayerManager.instance.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Scene_CharacterSelect", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        LobbyPanel.SetActive(false);
        MultiplayerManager.instance.StartClient();
    }

    public void ReturnToLobby()
    {
        LobbyPanel.SetActive(true);

        createGamePanel.SetActive(false);
        messageWindow.Hide();
    }

    private void OnDestroy()
    {
        MultiplayerManager.instance.OnTryingToConnect -= MultiplayerManager_OnTryingToConnect;
        MultiplayerManager.instance.OnFailedToConnect -= MultiplayerManager_OnFailedToConnect;

        MultiplayerLobby.instance.OnCreateLobbyStarted -= MultiplayerLobby_OnCreateLobbyStarted;
        MultiplayerLobby.instance.OnCreateLobbyFailed -= MultiplayerLobby_OnCreateLobbyFailed;

        MultiplayerLobby.instance.OnJoinStarted -= MultiplayerLobby_OnJoinStarted;
        MultiplayerLobby.instance.OnJoinFailed -= MultiplayerLobby_OnJoinFailed;
        MultiplayerLobby.instance.OnQuickJoinFailed -= MultiplayerLobby_OnQuickJoinFailed;

        MultiplayerLobby.instance.OnGamesCountChanged -= MultiplayerLobby_OnGamesCountChanged;
    }
}
