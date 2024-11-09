using CobaPlatinum.DebugTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkSceneManager : NetworkBehaviour
{
    #region Singleton
    public static NetworkSceneManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            CP_DebugWindow.LogWarning(this, "More than one instance of NetworkSceneManager found!");

            return;
        }

        instance = this;

        DontDestroyOnLoad(instance.gameObject);
    }
    #endregion

    [SerializeField] private Animator loadingScreenAnimator;
    [SerializeField] private Image loadingScreenBackground;
    [SerializeField] private float transitionTime = 1;

    public void LoadNetworkScene(string _sceneName, bool _showLoadingScreen)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_showLoadingScreen)
            StartCoroutine(LoadNetworkSceneWithLoadingScreen(_sceneName));
        else
            NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    IEnumerator LoadNetworkSceneWithLoadingScreen(string _sceneName)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ShowLoadingScreen();

        yield return new WaitForSeconds(transitionTime);
        
        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    public void LoadScene(string _sceneName, bool _showLoadingScreen)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_showLoadingScreen)
            StartCoroutine(LoadSceneWithLoadingScreen(_sceneName));
        else
            SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    IEnumerator LoadSceneWithLoadingScreen(string _sceneName)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ShowLoadingScreen();

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    public void ShowLoadingScreen()
    {
        if (!IsServer) return;

        SetLoadingScreenColor();

        loadingScreenAnimator.SetTrigger("start");

        ShowLoadingScreenClientRpc();
    }

    public void SetLoadingScreenColor()
    {
        loadingScreenBackground.color = 
            MultiplayerManager.instance.GetColorFromIndex(
                MultiplayerManager.instance.GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId).GetPlayerColorIndex());
    }

    [ClientRpc]
    public void ShowLoadingScreenClientRpc()
    {
        SetLoadingScreenColor();

        loadingScreenAnimator.SetTrigger("start");
    }

    [ClientRpc]
    public void EndLoadingScreenClientRpc()
    {
        EndLoadingScreen();
    }

    public void EndLoadingScreen()
    {
        loadingScreenAnimator.SetTrigger("end");
    }
}
