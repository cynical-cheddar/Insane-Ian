using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretFollowTarget : MonoBehaviour
{
    public GameObject target;
    public float trackingSpeed = 100;
    public float deadZone = 8;
    public float deadZoneTrackingSpeed = 10;
    public float upTraverse = 45;
    public float downTraverse = 45;
    public bool inDeadZone = true;
    private List<TurretBody> bodyComponents;

    // Start is called before the first frame update
    void Start()
    {
        bodyComponents = new List<TurretBody>();
        GetComponentsInChildren<TurretBody>(bodyComponents);
        foreach (TurretBody body in bodyComponents) {
            body.SetupParents();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 dir = target.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(dir, transform.parent.up);

        float missAngle = Quaternion.Angle(transform.rotation, targetRotation);
        if (missAngle > deadZone) {
            float rotationTime = Time.fixedDeltaTime;
            float maxRotation = trackingSpeed * rotationTime;
            if (maxRotation > (missAngle - deadZone)) {
                maxRotation = (missAngle - deadZone);
                rotationTime -= maxRotation / trackingSpeed;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotation);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deadZoneTrackingSpeed * rotationTime);
            }
            else transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotation);
            if (inDeadZone == true) Debug.Log("Left dead zone.");
            inDeadZone = false;
        }
        else {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deadZoneTrackingSpeed * Time.fixedDeltaTime);
            if (inDeadZone == false) Debug.Log("Entered dead zone.");
            inDeadZone = true;
        }

        float pitch = transform.localEulerAngles.x;
        if (pitch > 180) pitch -= 360;
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(pitch, -upTraverse, downTraverse), transform.localEulerAngles.y, transform.localEulerAngles.z);
        foreach (TurretBody body in bodyComponents) {
            body.UpdateAngle();
        }
    }
}
