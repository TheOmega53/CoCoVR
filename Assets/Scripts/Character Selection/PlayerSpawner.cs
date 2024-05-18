using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawner : NetworkBehaviour
{
    public Vector3 SpawnPosition;    

    [SerializeField] private CharacterDatabase characterDatabase;
    public override void OnNetworkSpawn()
    {
        GameManager.Instance.transform.position = SpawnPosition;

        Debug.LogWarning("Called");
        if (!IsServer)
        {
            return;
        }
        
        foreach (var client in GameManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if(character != null)
            {
                var characterInstance = Instantiate(character.GameplayPrefab, SpawnPosition, Quaternion.identity,gameObject.transform);                


                characterInstance.SpawnAsPlayerObject(client.Value.clientId);


                /*
                var followInputComponent = characterInstance.gameObject.GetComponent<IKTargetFollowVRRig>();
                var playerGazeTracker = characterInstance.gameObject.GetComponent <PlayerGazeTracker>();                
                if (client.Value.clientId == NetworkManager.Singleton.LocalClientId)
                {
                    followInputComponent.enabled = true;
                    playerGazeTracker.enabled = false;
                }
                else
                {
                    followInputComponent.enabled = false;
                    playerGazeTracker.enabled = true;
                }

                */
            }
        }
    }
}