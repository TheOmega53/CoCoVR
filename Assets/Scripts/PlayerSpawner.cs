using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawner : NetworkBehaviour
{
    public Vector3 hostSpawnPosition;
    public Vector3 clientSpawnPosition;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.transform.position = new Vector3(hostSpawnPosition.x,hostSpawnPosition.y,hostSpawnPosition.z);
        }
        else
        {
            gameObject.transform.position = new Vector3(clientSpawnPosition.x, clientSpawnPosition.y, clientSpawnPosition.z);
        }
    }
}