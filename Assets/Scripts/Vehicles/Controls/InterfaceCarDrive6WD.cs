using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceController6 : MonoBehaviour, IDrivable {
    // Start is called before the first frame update

    [Header("Wheel Colliders:")]
    public WheelCollider frontLeftW;
    public WheelCollider frontRightW;
    public WheelCollider rearLeft1W;
    public WheelCollider rearLeft2W;
    public WheelCollider rearRight1W;
    public WheelCollider rearRight2W;
    [Space(5)]

    [Header("Wheel Geometry Transforms")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeft1T;
    public Transform rearLeft2T;
    public Transform rearRight1T;
    public Transform rearRight2T;
    [Space(5)]

    [Header("Main Car")]
    public Rigidbody carRB;
    public Transform carTransform;
    [Space(5)]

    [Header("Force Parameters")]
    [Range(12, 35)]
    public float maxSteerAngle = 20;
    [Range(1000, 10000)]
    public float motorTorque = 6000;
    [Range(2000, 20000)]
    public float brakeTorque = 8000;
    [Range(0, 30000)]
    public float brakeForce = 16000;
    [Range(0, 5)]
    public float steerRate = 1.0f;
    [Range(0.01f, 0.5f)]
    public float steerRateCoefficent = 0.05f;
    public Vector3 addedDownforce;

    //direction is -1 for left and +1 for right, 0 for center
    void IDrivable.Steer(int targetDirection) {
        float targetAngle;
        float delta;
        float steerAngle;
        float horizontalVelocity;
        float newSteerRate;

        //Get the current steer angle
        steerAngle = frontLeftW.steerAngle;

        //targetAngle is the angle we want to tend towards
        targetAngle = targetDirection * maxSteerAngle;

        //Get the velocity in x and z dimensions
        horizontalVelocity = Mathf.Sqrt(Mathf.Pow(carRB.velocity.x, 2) + Mathf.Pow(carRB.velocity.z, 2));

        //set the steer rate to the minimum of normal steer rate and adjusted steer rate
        newSteerRate = steerRate / (steerRateCoefficent * horizontalVelocity);
        newSteerRate = Mathf.Min(newSteerRate, steerRate);

        //if the steer rate is less than the distance between target angle and current steering angle, set that to delta else only move the given distance.
        if (newSteerRate < Mathf.Abs(targetAngle - steerAngle)) {
            delta = newSteerRate;
        } else {
            delta = Mathf.Abs(targetAngle - steerAngle);
        }

        //if the target is zero return to centre
        if (!(targetAngle == 0)) {
            delta *= targetDirection;
        } else if (steerAngle > 0) {
            delta *= -1;
        }


        //set the steer angle
        steerAngle += delta;
        frontLeftW.steerAngle = steerAngle;
        frontRightW.steerAngle = steerAngle;

    }

    void IDrivable.Accellerate() {
        //assume accellerating
        frontLeftW.motorTorque  = motorTorque;
        frontRightW.motorTorque = motorTorque;
        rearLeft1W.motorTorque  = motorTorque;
        rearLeft2W.motorTorque  = motorTorque;
        rearRight1W.motorTorque = motorTorque;
        rearRight2W.motorTorque = motorTorque;
        if (transform.InverseTransformDirection(carRB.velocity).z > -4) {
            frontLeftW.motorTorque  = motorTorque;
            frontRightW.motorTorque = motorTorque;
            rearLeft1W.motorTorque  = motorTorque;
            rearLeft2W.motorTorque  = motorTorque;
            rearRight1W.motorTorque = motorTorque;
            rearRight2W.motorTorque = motorTorque;

        } else {
            ((IDrivable)this).Brake();
        }

    }
    void IDrivable.Reverse() {

        

        //check if needing to reverse or brake first
        if (transform.InverseTransformDirection(carRB.velocity).z < 4) {
            frontLeftW.motorTorque = -motorTorque;
            frontRightW.motorTorque = -motorTorque;
            rearLeft1W.motorTorque = -motorTorque;
            rearLeft2W.motorTorque = -motorTorque;
            rearRight1W.motorTorque = -motorTorque;
            rearRight2W.motorTorque = -motorTorque;
        } else {
            ((IDrivable)this).Brake();
        }


    }

    void IDrivable.Brake() {

        //brake all wheels
        frontLeftW.brakeTorque  = brakeTorque;
        frontRightW.brakeTorque = brakeTorque;
        rearLeft1W.brakeTorque  = brakeTorque;
        rearLeft2W.brakeTorque  = brakeTorque;
        rearRight1W.brakeTorque = brakeTorque; 
        rearRight2W.brakeTorque = brakeTorque;

        //if all wheels grounded, add additional brake force
        if (AllWheelsGrounded()) {
            if (transform.InverseTransformDirection(carRB.velocity).z < 0) {
                carRB.AddForce(carTransform.forward * brakeForce);
            } else {
                carRB.AddForce(carTransform.forward * -brakeForce);
            }
        }

    }

    private bool AllWheelsGrounded() {
        if (frontLeftW.isGrounded & frontRightW.isGrounded & rearLeft1W.isGrounded & rearRight1W.isGrounded & rearLeft2W.isGrounded & rearRight2W.isGrounded) {
            return true;
        } else return false;
    }

    void IDrivable.UpdateWheelPoses() {
        //make geometry match collider position
        UpdateWheelPose(frontLeftW,  frontLeftT,  true);
        UpdateWheelPose(frontRightW, frontRightT, false);
        UpdateWheelPose(rearLeft1W,  rearLeft1T,  true);
        UpdateWheelPose(rearLeft2W,  rearLeft2T,  true);
        UpdateWheelPose(rearRight1W, rearRight1T, false);
        UpdateWheelPose(rearRight2W, rearRight2T, false);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform transform, bool flip) {
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

    void IDrivable.StopAccellerate() {
        frontLeftW.motorTorque  = 0;
        frontRightW.motorTorque = 0;
        rearLeft1W.motorTorque  = 0;
        rearLeft2W.motorTorque  = 0;
        rearRight1W.motorTorque = 0;
        rearRight2W.motorTorque = 0;

    }

    void IDrivable.StopBrake() {
        frontLeftW.brakeTorque  = 0;
        frontRightW.brakeTorque = 0;
        rearLeft1W.brakeTorque  = 0;
        rearLeft2W.brakeTorque  = 0;
        rearRight1W.brakeTorque = 0;
        rearRight2W.brakeTorque = 0;

    }

    void IDrivable.StopSteer() {
        //steer towards zero
        ((IDrivable)this).Steer(0);
    }
}
