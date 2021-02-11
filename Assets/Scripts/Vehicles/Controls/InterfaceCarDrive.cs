using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceCarDrive : MonoBehaviour, IDrivable {
    // Start is called before the first frame update

    [Header("Wheel Colliders:")]
    public WheelCollider frontLeftW;
    public WheelCollider frontRightW;
    public WheelCollider rearLeftW;
    public WheelCollider rearRightW;
    [Space(5)]

    [Header("Wheel Geometry Transforms")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;
    [Space(5)]

    [Header("Main Car")]
    public Rigidbody carRB;
    public Transform carTransform;
    [Space(5)]

    [Header("Force Parameters")]
    [Range(12,35)]
    public float maxSteerAngle = 20;
    [Range(1000, 10000)]
    public float motorTorque = 4000;
    [Range(2000, 20000)]
    public float brakeTorque = 4000;
    [Range(0, 30000)]
    public float brakeForce = 16000;
    [Range(0, 5)]
    public float steerRate = 1.0f;
    [Range(0.01f,0.5f)]
    public float steerRateCoefficent = 0.05f;
    public Vector3 addedDownforce;

    //direction is +1 for right and -1 for left
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



        steerAngle += delta;
        frontLeftW.steerAngle = steerAngle;
        frontRightW.steerAngle = steerAngle;

    }

    void IDrivable.Accellerate() {
        
        //frontLeftW.motorTorque = motorTorque;
        //frontRightW.motorTorque = motorTorque;
        rearLeftW.motorTorque = motorTorque;
        rearRightW.motorTorque = motorTorque;
        
    }
    void IDrivable.Reverse() {

        //frontLeftW.motorTorque = motorTorque;
        //frontRightW.motorTorque = motorTorque;
        rearLeftW.motorTorque = -motorTorque;
        rearRightW.motorTorque = -motorTorque;


    }

    void IDrivable.Brake() {
        //frontLeftW.brakeTorque = brakeTorque;
        //frontRightW.brakeTorque = brakeTorque;
        rearLeftW.brakeTorque = brakeTorque;
        rearRightW.brakeTorque = brakeTorque;
        if (AllWheelsGrounded()) {
            if (transform.InverseTransformDirection(carRB.velocity).z < 0) {
                carRB.AddForce(carTransform.forward * brakeForce);
            } else {
                carRB.AddForce(carTransform.forward * -brakeForce);
            }
        }

    }

    private bool AllWheelsGrounded() {
        if (frontLeftW.isGrounded & frontRightW.isGrounded & rearLeftW.isGrounded & rearRightW.isGrounded) {
            return true;
        } else return false;
    }

    void IDrivable.UpdateWheelPoses() {
        UpdateWheelPose(frontLeftW, frontLeftT, true);
        UpdateWheelPose(frontRightW, frontRightT, false);
        UpdateWheelPose(rearLeftW, rearLeftT, true);
        UpdateWheelPose(rearRightW, rearRightT, false);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform transform, bool flip) {
        Vector3 pos = transform.position;
        Quaternion quat = transform.rotation;

        collider.GetWorldPose(out pos, out quat);

        transform.position = pos;
        transform.rotation = quat;
        if (flip) {
            transform.rotation *= new Quaternion(0, 0, -1, 0);
        }
    }

    void IDrivable.StopAccellerate() {
        frontLeftW.motorTorque = 0;
        frontRightW.motorTorque = 0;
        rearLeftW.motorTorque = 0;
        rearRightW.motorTorque = 0;

    }

    void IDrivable.StopBrake() {
        frontLeftW.brakeTorque = 0;
        frontRightW.brakeTorque = 0;
        rearLeftW.brakeTorque = 0;
        rearRightW.brakeTorque = 0;

    }

    void IDrivable.StopSteer() {
        ((IDrivable)this).Steer(0);
    }
}

