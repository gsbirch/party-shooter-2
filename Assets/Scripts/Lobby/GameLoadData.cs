using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// make a struct for player data
public struct PlayerData : INetworkSerializeByMemcpy {
    public NetworkString username;
    public Color color;
}

public class GameLoadData : NetworkBehaviour
{
    // Only the server keeps track of this info
    // In the future, this list will not exist
    public List<Color> playerColors = new() {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.cyan,
        Color.magenta,
    };
    public int colorInt = 0;

    public Color GetNextColor() {
        Color color = playerColors[colorInt];
        colorInt = (colorInt + 1) % playerColors.Count;
        return color;
    }

    public static GameLoadData Instance;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            // save this object when we load a new scene
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        usernames = new NetworkList<NetworkString>();
        clientIDs = new NetworkList<ulong>();
    }

    public NetworkList<NetworkString> usernames;
    public NetworkList<ulong> clientIDs;

    public Dictionary<ulong, PlayerData> clientIDToData = new();

    // This event is invoked whenever some data in the GameLoadData has changed
    // This is necessary because all the Network variables are not accessible
    // until the host has started, and we want to bind before that
    public event Action OnGameLoadDataChanged;

    public override void OnNetworkSpawn() {
        usernames.OnListChanged += (NetworkListEvent<NetworkString> changeEvent) => {
            OnGameLoadDataChanged?.Invoke();
        };
        base.OnNetworkSpawn();
    }

    // In the future, this should create the data object by loading a save if there is one
    [Rpc(SendTo.Server)]
    public void RegisterPlayerRpc(NetworkString username, ulong clientid) {
        Debug.Log("Register Player " + clientid);
        usernames.Add(username);
        clientIDs.Add(clientid);

        PlayerData data = new() {
            username = username.Value,
            color = GetNextColor()
        };
        clientIDToData[clientid] = data;
    }

    public void UnregisterPlayer(ulong clientid) {
        PlayerData data = clientIDToData[clientid];
        usernames.Remove(data.username);
        clientIDs.Remove(clientid);
        clientIDToData.Remove(clientid);
    }

    // Function to make it easy to send all players a notification
    // Can't be in notifications because it needs to be a NetworkBehaviour
    [Rpc(SendTo.Everyone)]
    public void BroadcastNotificationRpc(NetworkString message, Color color, float time = 5.0f) {
        Notifications.Instance.ShowNotification(message.Value, color, time);
    }
}
