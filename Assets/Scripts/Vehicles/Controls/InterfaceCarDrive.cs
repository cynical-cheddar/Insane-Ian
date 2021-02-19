using UnityEngine;

public class InterfaceCarDrive : MonoBehaviour, IDrivable {
    // Start is called before the first frame update


    [Header("Wheel Colliders:")]
    public WheelCollider frontLeftW;
    public WheelCollider frontRightW;
    public WheelCollider rearLeftW;
    public WheelCollider rearRightW;
    public bool is4WD = true;
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
    [Range(12, 35)]
    public float maxSteerAngle = 20;
    [Range(1000, 20000)]
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
        //check if needing to brake or accellerate
        if (transform.InverseTransformDirection(carRB.velocity).z > -4) {
            ((IDrivable)this).StopBrake();
            rearLeftW.motorTorque = motorTorque;
            rearRightW.motorTorque = motorTorque;
            if (is4WD) {
                frontLeftW.motorTorque = motorTorque;
                frontRightW.motorTorque = motorTorque;
            }
        } else {
            ((IDrivable)this).Brake();
        }

    }
    void IDrivable.Reverse() {
        //check if needing to reverse or brake first
        if (transform.InverseTransformDirection(carRB.velocity).z < 4) {
            ((IDrivable)this).StopBrake();
            rearLeftW.motorTorque = -motorTorque;
            rearRightW.motorTorque = -motorTorque;
            if (is4WD) {
                frontLeftW.motorTorque = -motorTorque;
                frontRightW.motorTorque = -motorTorque;
            }
        } else {
            ((IDrivable)this).Brake();
        }


    }

    void IDrivable.Brake() {
        //brake all wheels
        frontLeftW.brakeTorque = brakeTorque;
        frontRightW.brakeTorque = brakeTorque;
        rearLeftW.brakeTorque = brakeTorque;
        rearRightW.brakeTorque = brakeTorque;

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
        if (frontLeftW.isGrounded & frontRightW.isGrounded & rearLeftW.isGrounded & rearRightW.isGrounded) {
            return true;
        } else return false;
    }

    void IDrivable.UpdateWheelPoses() {
        //make geometry match collider position
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
        //if wheel is on the opposite side of the car, flip the wheel
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
        //steer towards 0
        ((IDrivable)this).Steer(0);
    }
}

