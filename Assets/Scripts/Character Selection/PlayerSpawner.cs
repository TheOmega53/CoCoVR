using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawner : NetworkBehaviour
{
    public Vector3 SpawnPosition;

    [SerializeField] private CharacterDatabase characterDatabase;
    public override void OnNetworkSpawn()
    {
        Debug.LogWarning("Called");
        if (!IsServer)
        {
            //return;
        }

        Debug.Log("player count: " + GameManager.Instance.ClientData.Count);
        foreach (var client in GameManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if(character != null)
            {
                var characterInstance = Instantiate(character.GameplayPrefab, SpawnPosition, Quaternion.identity,gameObject.transform);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}