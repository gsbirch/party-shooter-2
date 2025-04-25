using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public enum ControlType {
    GUI,
    InGame
}
enum InteractionType {
    None,
    Looking,
    Interacting,
} 

public enum InteractButton {
    None,
    Primary,
    Secondary,
    Teritary

}

// This struct is passed during interaction to the interactable object
// It is the interactable's responsibility to handle this data
// especially if it changes
public struct InteractData {
    public InteractButton button;
    public Vector3 hitPoint;
} 

public class PlayerController : NetworkBehaviour
{
    public float interactionDistance;

    public static PlayerController LocalPlayer;
    public PlayerActions InputActions;
    public static event Action LocalPlayerInitialized;
    public Animator modelAnimator;

    public GameObject playerModel;

    public TextMeshPro nameText;

    public NetworkVariable<PlayerData> playerData;

    private IInteractable currentInteractable = null;
    private InteractionType currentInteractType = InteractionType.None;


    private void Awake() {
        Debug.Log("Player Awake");
    }

    private void Start() {
        Debug.Log("Player Start");
        if (IsServer) {
            playerData.Value = GameLoadData.Instance.clientIDToData[OwnerClientId];
           
        }
        // Anytime a new one is made, we call this again
        // This means any player who joins late causes a resync
        // This allows them to recieve updated appearances
        else {
            SyncAppearanceRpc();
        }
        if (IsLocalPlayer) {
            LocalPlayerInitialized?.Invoke();
        }
    }
    // ===== INITALIZATION =====
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        Debug.Log("Player NetworkSpawn " + OwnerClientId);

        if (IsLocalPlayer) {
            InputActions = new PlayerActions();
            InputActions.ExternalControls.Enable();
            InputActions.ExternalControls.Pause.performed += ctx => {
                PauseMenu.Instance.TogglePauseMenu();

            };
            InputActions.ExternalControls.Fullscreen.performed += ctx => {
                Screen.fullScreen = !Screen.fullScreen;
            };

            // Other components should connect to this with OnNetworkSpawn
            GetComponent<PlayerMovement>().enabled = true;
            GetComponent<PlayerMovement>().ConnectInput();
            
            LocalPlayer = this;
            
            SetControlType(ControlType.InGame);
            playerModel.transform.Find("Alpha_Surface").GetComponent<Renderer>().enabled = false;
            playerModel.transform.Find("Alpha_Joints").GetComponent<Renderer>().enabled = false;
            nameText.enabled = false;
            
            InputActions.ExternalControls.ShowDebugMenu.performed += ctx => {
                PauseMenu.Instance.ToggleDebugMenu();
            };
            GameLoadData.Instance.OnGameLoadDataChanged += () => {
                Debug.Log("I have detected a change in the GameLoadData, here are the active players:");
                foreach(NetworkString s in GameLoadData.Instance.usernames) {
                    Debug.Log(s.ToString());
                }

            };
        }
        
    }

    [Rpc(SendTo.Server)]
    public void SyncAppearanceRpc() {
        SetNameRpc(playerData.Value.username.Value);
        SetColorRpc(playerData.Value.color);
    }

    [Rpc(SendTo.Everyone)]
    public void SetNameRpc(string name) {
        nameText.text = name;
    }

    [Rpc(SendTo.Everyone)]
    public void SetColorRpc(Color color) {
        nameText.faceColor = color;
        // find the game object with name "PlayerModel" and set its color
        playerModel.transform.Find("Alpha_Surface").GetComponent<Renderer>().material.color = color;
    }


    // ===== CONTROL =====
    // On the Client
    public void SetControlType(ControlType controlType) {
        switch (controlType) {
            case ControlType.GUI:
                InputActions.PlayerControls.Disable();
                // show the mouse
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                // disable looking around
                // This is kind of a work around since the camera is actually reading a different asset than
                // the players actual InputAction class
                for (var i = 0; i < GetComponent<PlayerMovement>().cameraObj.GetComponent<CinemachineInputAxisController>().Controllers.Count; i++) {
                    GetComponent<PlayerMovement>().cameraObj.GetComponent<CinemachineInputAxisController>().Controllers[i].Enabled = false;
                }
                break;
            case ControlType.InGame:
                InputActions.PlayerControls.Enable();
                // hide the mouse
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                // enable looking around
                for (var i = 0; i < GetComponent<PlayerMovement>().cameraObj.GetComponent<CinemachineInputAxisController>().Controllers.Count; i++) {
                    GetComponent<PlayerMovement>().cameraObj.GetComponent<CinemachineInputAxisController>().Controllers[i].Enabled = true;
                }
                break;
        }
    }

    // ====== INTERACTION =====
    private void Update() {
        if (!IsLocalPlayer) return;
        var interactionData = new InteractData {
            button = GetInteractButton(),
            hitPoint = Vector3.zero
        };
        // --- INTERACTION ---
        if (!Camera.main) return;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance) && hit.collider.TryGetComponent(out IInteractable interactable)) {
            interactionData.hitPoint = hit.point;
            // stop interacting with the current object
            if (currentInteractType != InteractionType.None && currentInteractable != interactable) {
                if (currentInteractType == InteractionType.Interacting) currentInteractable.StopInteract(this);
                currentInteractable.LookAway(this);

                // we reset this to reuse code as if there was no previous object
                currentInteractable = null;
                currentInteractType = InteractionType.None;
            }

            // start looking at the new object
            if (currentInteractType == InteractionType.None) {
                currentInteractable = interactable;
                // removed since it will be called in the next if statement
                currentInteractable.LookAt(this, interactionData);
                currentInteractType = InteractionType.Looking;
            }

            if (currentInteractType == InteractionType.Looking) {
                if (PressedThisFrame() && currentInteractable.StartInteract(this, interactionData)) {
                    currentInteractType = InteractionType.Interacting;
                }
                else {
                    currentInteractable.LookAt(this, interactionData);
                }
            }

            if (currentInteractType == InteractionType.Interacting) {
                if (interactionData.button != InteractButton.None) {
                    // We're going to update the interaction
                    // We are using the same interact button we used originally
                    // but if the object says we can't interact anymore, we stop interacting
                    if (!currentInteractable.UpdateInteract(this, interactionData)) {
                        currentInteractable.StopInteract(this);
                        currentInteractType = InteractionType.Looking;
                    }
                }
                // the player has let go of the button, thus they go back to just looking
                // if they somehow switched buttons, we just let that update happen next frame
                else {
                    currentInteractable.StopInteract(this);
                    currentInteractType = InteractionType.Looking;
                }
            }
        }
        else {
            // we didn't see anything so we need to stop interacting with the previous object, if there was one
            switch (currentInteractType) {
                case InteractionType.Looking:
                    currentInteractable.LookAway(this);
                    break;
                case InteractionType.Interacting:
                    currentInteractable.StopInteract(this);
                    currentInteractable.LookAway(this);
                    break;
            }
            currentInteractType = InteractionType.None;
            currentInteractable = null;
        }
    }

    // because of the structure of this function, buttons have a priority of Primary > Secondary > Tertiary
    // so if the player is holding down the secondary, but then presses the primary, the primary will be the button
    InteractButton GetInteractButton() {
        if (InputActions.PlayerControls.PrimaryInteract.IsPressed()) {
            return InteractButton.Primary;
        }
        else if (InputActions.PlayerControls.SecondaryInteract.IsPressed()) {
            return InteractButton.Secondary;
        }
        else if (InputActions.PlayerControls.TertiaryInteract.IsPressed()) {
            return InteractButton.Teritary;
        }
        else {
            return InteractButton.None;
        }
    }

    bool PressedThisFrame() {
        return InputActions.PlayerControls.PrimaryInteract.triggered || 
                InputActions.PlayerControls.SecondaryInteract.triggered || 
                InputActions.PlayerControls.TertiaryInteract.triggered;
    }
}
