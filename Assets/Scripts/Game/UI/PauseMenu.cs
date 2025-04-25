using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    // Singleton
    public static PauseMenu Instance;
    private void Awake() {
        Debug.Log("PauseMenu Awake");
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        GetComponent<UIDocument>().rootVisualElement.visible = false;
        Instance = this;
    }

    SliderInt sensitivitySlider;
    Label sensitivityText;
    SliderInt volumeSlider;
    Label volumeText;
    Button quitGame;
    Button exitMenu;
    bool paused = false;

    // Debugging
    VisualElement debugPanel;
    Button endMatchButton;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PauseMenu Start");
        var root = GetComponent<UIDocument>().rootVisualElement;
        sensitivitySlider = root.Q<SliderInt>("SensitivitySlider");
        sensitivityText = root.Q<Label>("SensitivityText");
        volumeSlider = root.Q<SliderInt>("VolumeSlider");
        volumeText = root.Q<Label>("VolumeText");
        quitGame = root.Q<Button>("QuitGame");
        exitMenu = root.Q<Button>("ExitMenu");
        debugPanel = root.Q<VisualElement>("DebugPanel");
        endMatchButton = root.Q<Button>("EndMatch");

        sensitivitySlider.RegisterValueChangedCallback((evt) => {
            sensitivityText.text = "Sensitivity: " + evt.newValue;
            PlayerController.LocalPlayer.GetComponent<PlayerMovement>().cameraObj.GetComponent<CinemachineInputAxisController>().Controllers[0].Input.Gain = CalculateSensitivity(evt.newValue);
            PlayerController.LocalPlayer.GetComponent<PlayerMovement>().cameraObj.GetComponent<CinemachineInputAxisController>().Controllers[1].Input.Gain = -CalculateSensitivity(evt.newValue);
            PlayerPrefs.SetInt("Sensitivity", evt.newValue);
        });

        volumeSlider.RegisterValueChangedCallback((evt) => {
            volumeText.text = "Volume: " + evt.newValue;
            AudioListener.volume = evt.newValue / 100f;
            PlayerPrefs.SetInt("Volume", evt.newValue);
        });

        quitGame.clicked += () => {
            Application.Quit();
        };
        exitMenu.clicked += () => {
            SetPauseMenu(false);
        };
        endMatchButton.clicked += () => {
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
        };

        // WE ARE SO BACK
        PlayerController.LocalPlayerInitialized += () => {
            if (PlayerPrefs.HasKey("Sensitivity")) {
                sensitivitySlider.value = PlayerPrefs.GetInt("Sensitivity");
            } else {
                sensitivitySlider.value = 10;
            }
            if (PlayerPrefs.HasKey("Volume")) {
                volumeSlider.value = PlayerPrefs.GetInt("Volume");
            } else {
                volumeSlider.value = 50;
            }
        };
    }


    public void SetPauseMenu(bool pause) {
        paused = pause;
        if (pause) {
            GetComponent<UIDocument>().rootVisualElement.visible = true;
            PlayerController.LocalPlayer.SetControlType(ControlType.GUI);
        } else {
            GetComponent<UIDocument>().rootVisualElement.visible = false;
            PlayerController.LocalPlayer.SetControlType(ControlType.InGame);
        }
    }

    public void TogglePauseMenu() {
        SetPauseMenu(!paused);
    }
    public void ToggleDebugMenu() {
        debugPanel.style.display = (debugPanel.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public float CalculateSensitivity(float value) {
        return Map(value, 1, 30, 1, 30);
    }
    public float Map(float value, float x1, float x2, float y1, float y2) {
        // Handle edge cases where x1 and x2 are the same to avoid division by zero
        if (Mathf.Abs(x2 - x1) < Mathf.Epsilon) {
            Debug.LogWarning("Input range cannot have the same upper and lower bounds.");
            return y1; // Return the lower bound of the output range as a fallback
        }

        return y1 + ((value - x1) * (y2 - y1) / (x2 - x1));
    }
}
