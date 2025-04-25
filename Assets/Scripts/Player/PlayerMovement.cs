
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerMovement : NetworkBehaviour {

    
    public float speed = 4f;
    public float speedMultiplier = 1f;
    public float groundDrag;
    public Transform forward;
    public GameObject cameraObj;

   [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;

    InputAction move;
    bool isGrounded;

    Vector3 inputDir;

    Rigidbody rb;


    public void Start() {
        //animator = transform.Find("Y Bot").GetComponent<Animator>();
        // make the cameraRig the parent of the camera

        cameraObj.GetComponent<CinemachineCamera>().Priority = 10;
    }

    public void ConnectInput() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        var controller = GetComponent<PlayerController>();

        move = controller.InputActions.PlayerControls.Move;
        controller.InputActions.PlayerControls.Jump.performed += Jump;

        
    }

    private void Update() {
        if (rb == null) {
            return;
        }
        if (move == null) return;

        // get main camera's rotation
        if (!Camera.main) return;
        float horizontal = Camera.main.transform.rotation.eulerAngles.y;
        GetComponent<PlayerController>().playerModel.transform.rotation = Quaternion.Euler(0, horizontal, 0);

        // Smoothly interpolate the forward rotation
        Quaternion targetRotation = Camera.main.transform.rotation;
        forward.rotation = Quaternion.Lerp(forward.rotation, targetRotation, Time.deltaTime * 10); // Adjust the multiplier as needed

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Get input
        Vector2 inputVec = move.ReadValue<Vector2>();

        // This does not need to be normalized as it will automatically account for the magnitude of input.
        // Thus, it's magnitude <= 1
        // compute input direction by rotating input vector by camera's y rotation
        inputDir = Quaternion.Euler(0, horizontal, 0) * new Vector3(inputVec.x, 0, inputVec.y);

        Animate();

        SpeedControl();

        if (isGrounded) {
            rb.drag = groundDrag;
        }
        else {
            rb.drag = 0;
        }
    }

    private void FixedUpdate() {
        if (rb == null) {
            return;
        }
        MovePlayer();
    }

    void Animate() {
        var controller = GetComponent<PlayerController>();
        int moveInt = -1;


        // set animator direction
        if (inputDir.magnitude > 0) {
            Vector2 forwardVec = new(forward.forward.x, forward.forward.z);
            Vector2 dirVec = new(inputDir.x, inputDir.z);
            float angle = Vector2.SignedAngle(forwardVec, dirVec);
            // make negative angle value into positive one.
            if (angle < -45f) angle += 360.0f;
            moveInt = ((int)angle + 45) / 90;
        }
        GetComponent<PlayerController>().modelAnimator.SetInteger("MoveInt", moveInt);
        //GetComponent<PlayerController>().modelAnimator.GetComponent<NetworkAnimator>().Animator.SetInteger("MoveInt", moveInt);
    }

    void MovePlayer() {
        // Move character
        if (isGrounded)
            rb.AddForce(speed * 10f * inputDir, ForceMode.Force);
        else
            rb.AddForce(speed * airMultiplier * 10f * inputDir, ForceMode.Force);
    }

    void SpeedControl() {
        Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);
        // limit velocity if needed
        if (flatVel.magnitude > speed) {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (!isGrounded) return;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}