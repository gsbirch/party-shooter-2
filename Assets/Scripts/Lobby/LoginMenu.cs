using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoginMenu : MonoBehaviour
{
    // Singleton
    public static LoginMenu Instance;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    TextField username;
    TextField address;
    Button join;
    Button host;

    public string Username => username.value;
    string ip = "";
    ushort port = 7777;

    // Start is called before the first frame update
    void Start() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        username = root.Q<TextField>("username");
        address = root.Q<TextField>("address");
        join = root.Q<Button>("join");
        join.clicked += () => {
            ParseAddress();
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, port);
            NetworkManager.Singleton.StartClient();
        };

        host = root.Q<Button>("host");
        host.clicked += () => {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, port, "0.0.0.0");
            NetworkManager.Singleton.StartHost();
        };
    }

    void ParseAddress() {
        string[] parts = address.value.Split(':');
        ip = parts[0];

        // because SetConnectionData doesn't like localhost???
        if (ip == "localhost") {
            ip = "127.0.0.1";
        }
        if (parts.Length > 1) {
            port = ushort.Parse(parts[1]);
        }
    }
}
