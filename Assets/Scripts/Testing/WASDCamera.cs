using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDCamera : MonoBehaviour {

    public GameObject cam;
    public float cameraSensitivity = 2f;
    public float movementSpeed = 2.5f;
    public GameObject projectile;

    float pitch;
    float yaw;
    bool canFire = true;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // Adjust camera
        yaw += cameraSensitivity * Input.GetAxis("Mouse X");
        pitch -= cameraSensitivity * Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector3(Mathf.Clamp(pitch, -45, 45), yaw, 0f);

        // Adjust player position and rotation
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float updown = 0;
        if (Input.GetKey(KeyCode.Space)) updown = 1;
        else if (Input.GetKey(KeyCode.LeftShift)) updown = -1;
        Vector3 targetDirection = new Vector3(horizontal, updown, vertical);
        targetDirection = cam.transform.TransformDirection(targetDirection);

        transform.position += targetDirection * movementSpeed * Time.deltaTime;

        if (Input.GetMouseButton(0) && canFire) {
            shootProjectile(cam.transform.forward);
        }
    }

    void shootProjectile(Vector3 direction) {
        canFire = false;
        Invoke("reload", 0.1f);
        Instantiate(projectile, cam.transform.position, Quaternion.LookRotation(direction));
    }

    void reload() {
        canFire = true;
    }
}
