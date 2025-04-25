using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameInitializer : NetworkBehaviour {
    public GameObject playerPrefab;
    public GameObject[] spawnPositions;
    public int spawnIndex = 0;

    // Create the player prefabs
    public override void OnNetworkSpawn() {
        if (IsClient) {
            CreatePlayerRpc(NetworkManager.Singleton.LocalClientId);
        }
        base.OnNetworkSpawn();
    }

    // Now, each player requests their own prefab
    // This allows the scene to load first before
    // the prefab is created
    [Rpc(SendTo.Server)]
    public void CreatePlayerRpc(ulong clientID) {
        GameObject player = Instantiate(playerPrefab, spawnPositions[spawnIndex].transform.position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
        spawnIndex = (spawnIndex + 1) % spawnPositions.Length;
    }
}