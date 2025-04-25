using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnArea : NetworkBehaviour
{
    public GameObject[] spawnObjects;
    public float xRadius = 5.0f;
    public float yRadius = 5.0f;
    public float zRadius = 5.0f;
    public Color color = Color.green;

    // draw a gizmo to show the spawn area
    private void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, new Vector3(xRadius * 2, yRadius * 2, zRadius * 2));
    }

    public void SpawnObject() {
        if (!IsServer) {
            Debug.LogWarning("Only the server can spawn objects");
            return;
        }
        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-xRadius, xRadius),
            Random.Range(-yRadius, yRadius),
            Random.Range(-zRadius, zRadius)
        );
        // instantiate a random object from the list
        // and give it a random y rotation
        GameObject go = Instantiate(spawnObjects[Random.Range(0, spawnObjects.Length)], spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        go.GetComponent<NetworkObject>().Spawn();
    }
}
