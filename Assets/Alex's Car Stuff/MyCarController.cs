using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCarController : MonoBehaviour { 
    // Start is called before the first frame update
    public WheelCollider frontLeftW;
    public WheelCollider frontRightW;
    public WheelCollider rearLeftW;
    public WheelCollider rearRightW;

    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;
    public Transform car;

    public Rigidbody rb;
    public float maxSteerAngle = 30;
    public float motorTorque = 50;
    public float brakeTorque = 50;
    public float brakeForce = 15000;
    public float steerRate =2.0f;
    public float steerRateCoefficent = 0.9f;


    private void Steer() {
        float targetAngle;
        float inLeft = 0;
        float inRight = 0;
        float targetDirection;
        float delta;
        float steerAngle;
        float horizontalVelocity;
        float newSteerRate;

        steerAngle = frontLeftW.steerAngle;


        if (Input.GetKey(KeyCode.A)) {
            inLeft = 1;
        }
        if (Input.GetKey(KeyCode.D)) {
            inRight = 1;
        }

        //targetDirection is the direction of the input
        targetDirection = inRight - inLeft;
        Debug.Log("Target direction = " + targetDirection);
        //targetAngle is the angle we want to tend towards
        targetAngle = targetDirection*maxSteerAngle;

        horizontalVelocity = Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2));
        newSteerRate = steerRate / (steerRateCoefficent*horizontalVelocity);
        newSteerRate = Mathf.Min(newSteerRate, steerRate);

        //if the steer rate is less than the distance between target angle and current steering angle, set that to delta else only move the given distance.
        if (newSteerRate < Mathf.Abs(targetAngle - steerAngle)) {
            delta = newSteerRate;
        } else {
            delta = Mathf.Abs(targetAngle - steerAngle);
        }

        //if the target is zero, 
        if (!(targetAngle == 0)) {
            delta *= targetDirection;
            
        } else if(steerAngle > 0) {
            delta *= -1;
        }  



        steerAngle += delta;       
        frontLeftW.steerAngle = steerAngle;
        frontRightW.steerAngle = steerAngle;

    }

    private void Accellerate() {
        if (Input.GetKey(KeyCode.W)) {
            //frontLeftW.motorTorque = motorTorque;
            //frontRightW.motorTorque = motorTorque;
            rearLeftW.motorTorque = motorTorque;
            rearRightW.motorTorque = motorTorque;
        } else {
            frontLeftW.motorTorque = 0;
            frontRightW.motorTorque = 0;
            rearLeftW.motorTorque = 0;
            rearRightW.motorTorque = 0;
        }
    }

    private void Brake() {
        if (Input.GetKey(KeyCode.S)) {
            //frontLeftW.brakeTorque = brakeTorque;
            //frontRightW.brakeTorque = brakeTorque;
            rearLeftW.brakeTorque = brakeTorque;
            rearRightW.brakeTorque = brakeTorque;
            rb.AddForce(car.forward * -brakeForce);
        } else {
            frontLeftW.brakeTorque = 0;
            frontRightW.brakeTorque = 0;
            rearLeftW.brakeTorque = 0;
            rearRightW.brakeTorque = 0;
        } 
    }

    private void UpdateWheelPoses() {
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

    private void FixedUpdate() {
        //GetInput();
        Steer();
        Accellerate();
        Brake();
        UpdateWheelPoses();
    }
}

