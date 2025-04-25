using UnityEngine;

public class RotateAndBob : MonoBehaviour {
    public float rotationSpeed = 50f; // Speed of rotation around the local Y-axis
    public float bobSpeed = 5f; // Speed of the bobbing movement
    public float bobHeight = 0.5f; // Height of the bobbing movement

    private float startY; // Initial local Y position, acting as the bottom of the bobbing

    void Start() {
        // Store the initial local Y position of the GameObject
        startY = transform.localPosition.y;
    }

    void Update() {
        // Rotate the GameObject around its local Y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

        // Calculate the new local Y position using a sine wave for bobbing, starting from the bottom
        float newY = startY + bobHeight + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        // Apply the new local position
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}
