using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    // This is the interface for the interactable objects

    // LookAt is called when the player looks at the object
    void LookAt(PlayerController player, InteractData data);
    // LookAway is called when the player looks away from the object
    void LookAway(PlayerController player);

    // These functions should return true if the player can interact with the object

    // StartInteract is called when the player starts interacting with the object
    bool StartInteract(PlayerController player, InteractData data);
    // UpdateInteract is called every frame while the player is interacting with the object
    bool UpdateInteract(PlayerController player, InteractData data);
    // StopInteract is called when the player stops interacting with the object
    bool StopInteract(PlayerController player);
}
