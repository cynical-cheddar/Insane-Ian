using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelUpdater : MonoBehaviour {

    [Header("Wheel Colliders:")]
    public PhysXWheelCollider frontLeftW;
    public PhysXWheelCollider frontRightW;
    public PhysXWheelCollider rearLeftW;
    public PhysXWheelCollider rearRightW;
    [Space(5)]

    [Header("Wheel Geometry Transforms")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;

    // Update is called once per frame
    void Update()
    {
        UpdateWheelPose(frontLeftW, frontLeftT, true);
        UpdateWheelPose(frontRightW, frontRightT, false);
        UpdateWheelPose(rearLeftW, rearLeftT, true);
        UpdateWheelPose(rearRightW, rearRightT, false);
    }
    private void UpdateWheelPose(PhysXWheelCollider collider, Transform transform, bool flip) {
        Vector3 pos = transform.position;
        Quaternion quat = transform.rotation;

        collider.GetWorldPose(out pos, out quat);

        transform.position = pos;
        transform.rotation = quat;
        //if wheel is on the opposite side of the car, flip the wheel
        if (flip) {
            transform.rotation *= new Quaternion(0, 0, -1, 0);
        }
    }
}
