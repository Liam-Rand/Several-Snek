using Unity.Netcode;
using UnityEngine;

public class EndLoadingScreen : MonoBehaviour
{
    private void Start()
    {
        NetworkSceneManager.instance.EndLoadingScreen();
    }
}
