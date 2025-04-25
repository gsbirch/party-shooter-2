using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class NetworkHandler : MonoBehaviour
{
    private void Awake() {
        DontDestroyOnLoad(this);
    }

    private void Start() {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
    }

    private void OnDestroy() {
        // Since the NetworkManager can potentially be destroyed before this component, only
        // remove the subscriptions if that singleton still exists.
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }
    }
    void OnClientConnectedCallback(ulong clientId) {

        if (clientId == NetworkManager.Singleton.LocalClientId) {
            //UIDocument document = GetComponent<UIDocument>();
            // check if we're the host
            if (NetworkManager.Singleton.IsHost) {
                Notifications.Instance.ShowNotification("Successfully started hosting!", Color.green);
                NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else {
                Notifications.Instance.ShowNotification("Connected to the server!", Color.green);
            }

            //document.rootVisualElement.visible = false;
            GameLoadData.Instance.RegisterPlayerRpc(LoginMenu.Instance.Username, clientId);
        }
        if (!NetworkManager.Singleton.IsHost) {
            Notifications.Instance.ShowNotification("A player has joined!", Color.green, 3);
        }
        Debug.Log($"Client Connected: {clientId}");
    }

    void OnClientDisconnectedCallback(ulong clientId) {
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            // load the lobby scene
            SceneManager.LoadScene("Login");
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Notifications.Instance.ShowNotification("Disconnected from the server!", Color.red);
        }
        if (NetworkManager.Singleton.IsServer) {
            GameLoadData.Instance.UnregisterPlayer(clientId);
        }
        Debug.Log($"Client disconnected: {clientId}");
    }
}
