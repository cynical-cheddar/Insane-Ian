using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[VehicleScript(ScriptType.playerGunnerScript)]
[VehicleScript(ScriptType.aiGunnerScript)]
public class TurretFollowTarget : MonoBehaviour
{
    public GameObject target;
    public float trackingSpeed = 100;
    public float deadZone = 8;
    public float deadZoneTrackingSpeed = 10;
    public float upTraverse = 45;
    public float downTraverse = 45;
    public bool inDeadZone = true;
    public  Transform barrelTransform;
    private Quaternion virtualRotation;
    
    //private List<TurretBody> bodyComponents;

    // Start is called before the first frame update
    void Start()
    {
        //bodyComponents = new List<TurretBody>();
        //GetComponentsInChildren<TurretBody>(bodyComponents);
        //foreach (TurretBody body in bodyComponents) {
        //    body.SetupParents();
        //}
        if (barrelTransform == null) barrelTransform = transform.Find("BarrelHinge");
        
        virtualRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = target.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(dir, transform.parent.up);

        float missAngle = Quaternion.Angle(virtualRotation, targetRotation);
        if (missAngle > deadZone) {
            float rotationTime = Time.deltaTime;
            float maxRotation = trackingSpeed * rotationTime;
            if (maxRotation > (missAngle - deadZone)) {
                maxRotation = (missAngle - deadZone);
                rotationTime -= maxRotation / trackingSpeed;
                virtualRotation = Quaternion.RotateTowards(virtualRotation, targetRotation, maxRotation);
                virtualRotation = Quaternion.Slerp(virtualRotation, targetRotation, deadZoneTrackingSpeed * rotationTime);
            }
            else virtualRotation = Quaternion.RotateTowards(virtualRotation, targetRotation, maxRotation);
            inDeadZone = false;
        }
        else {
            virtualRotation = Quaternion.Slerp(virtualRotation, targetRotation, deadZoneTrackingSpeed * Time.deltaTime);
            inDeadZone = true;
        }

        transform.rotation = virtualRotation;
        float pitch = transform.localEulerAngles.x;
        if (pitch > 180) pitch -= 360;
        pitch = Mathf.Clamp(pitch, -upTraverse, downTraverse);
        transform.localRotation = Quaternion.Euler(pitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
        virtualRotation = transform.rotation;
        transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        barrelTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}
