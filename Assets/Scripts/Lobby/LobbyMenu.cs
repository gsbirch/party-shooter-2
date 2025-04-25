using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyMenu : NetworkBehaviour
{
    Button startGame;
    Label playerList;

    private void Awake() {
        var root = GetComponent<UIDocument>().rootVisualElement;
        startGame = root.Q<Button>("StartGame");
        playerList = root.Q<Label>("PlayerList");
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            startGame.clicked += StartGame;
            
        }
        else {
           startGame.style.display = DisplayStyle.None;
        }
        GameLoadData.Instance.OnGameLoadDataChanged += OnGameLoadDataChanged;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        GameLoadData.Instance.OnGameLoadDataChanged -= OnGameLoadDataChanged;
    }

    private void Start() {
        // Catch up on any previous updates
        // Also necessary for when we load back from a game
        OnGameLoadDataChanged();
    }

    private void StartGame() {
        if (IsServer) {
            NetworkManager.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else {
            Debug.Log("attempted to start game from client");
        }
    }

    public void OnGameLoadDataChanged() {
        string playerText = "";
        foreach (var player in GameLoadData.Instance.usernames) {
            playerText += player + "\n";
        }
        playerList.text = playerText;
    }
}
