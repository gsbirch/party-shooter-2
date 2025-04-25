using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public List<SpawnArea> spawnAreas;
    // can be used to toggle spawning on and off
    public bool spawning;
    public float spawnInterval = 30.0f;
    public int initialSpawnCount = 10;
    float spawnTimer = 0.0f;
    
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (!IsServer || !spawning) {
            return;
        }
        Populate(initialSpawnCount);
    }

    // Update is called once per frame
    void Update() {
        // if the spawn interval is negative, don't spawn anything
        if (!IsServer || !spawning) {
            return;
        }
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval) {
            spawnAreas[Random.Range(0, spawnAreas.Count)].SpawnObject();
            spawnTimer = 0.0f;
        }
    }

    public void Populate(int numObjects) {
        if (!IsServer) {
            Debug.LogWarning("Only the server can spawn objects");
            return;
        }
        var shuffled = Utils.Shuffle(spawnAreas);
        for (int i = 0; i < numObjects; i++) {
            if (i % shuffled.Count == 0) {
                shuffled = Utils.Shuffle(spawnAreas);
            }
            shuffled[i % shuffled.Count].SpawnObject();
        }
    }

    // Should be called on the server
    // might end up being an RPC?
    public void BeginSpawning() {
        spawning = true;
        Populate(initialSpawnCount);
    }
}
