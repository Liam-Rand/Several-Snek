using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstanceCleanup : MonoBehaviour
{
    private void Awake()
    {
        if(NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);

        if(MultiplayerManager.instance != null)
            Destroy(MultiplayerManager.instance.gameObject);

        if (MultiplayerLobby.instance != null)
            Destroy(MultiplayerLobby.instance.gameObject);

        if (NetworkSceneManager.instance != null)
            Destroy(NetworkSceneManager.instance.gameObject);
    }
}
