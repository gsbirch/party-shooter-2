using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboarding : MonoBehaviour {
    [SerializeField]
    bool rotateY;
    Vector3 cameraDir;

    private void Update() {
        if (!Camera.main) { return; }
        cameraDir = Camera.main.transform.forward;
        if (!rotateY) cameraDir.y = 0;
        transform.rotation = Quaternion.LookRotation(cameraDir);
    }
}
