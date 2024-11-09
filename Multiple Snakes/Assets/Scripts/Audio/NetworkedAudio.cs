using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkedAudio : NetworkBehaviour
{
    public void PlaySnakeHurtSound()
    {
        PlaySnakeHurtSoundServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaySnakeHurtSoundServerRpc()
    {
        //Server
        AudioManager.instance.GetSoundEffectsAudioSource().PlayOneShot(GameManager.instance.GetGameData().GetSnakeHurtAudio());

        //Clients
        PlaySnakeHurtSoundClientRpc();
    }

    [ClientRpc]
    public void PlaySnakeHurtSoundClientRpc()
    {
        AudioManager.instance.GetSoundEffectsAudioSource().PlayOneShot(GameManager.instance.GetGameData().GetSnakeHurtAudio());
    }
}
