using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretFollowTarget : MonoBehaviour
{
    public GameObject target;
    public float trackingSpeed = 100;
    public float deadZone = 8;
    public float deadZoneTrackingSpeed = 10;
    public bool inDeadZone = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = target.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        float missAngle = Quaternion.Angle(transform.rotation, targetRotation);
        if (missAngle > deadZone) {
            float rotationTime = Time.deltaTime;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deadZoneTrackingSpeed * Time.deltaTime);
            if (inDeadZone == false) Debug.Log("Entered dead zone.");
            inDeadZone = true;
        }
    }
}
