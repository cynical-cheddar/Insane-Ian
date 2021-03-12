using UnityEngine;

public class InterfaceCarDrive6W : InterfaceCarDrive, IDrivable {
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
    [Range(1000, 20000)]
    public float motorTorque = 3000;
    [Range(2000, 20000)]
    public float brakeTorque = 8000;
    [Range(0, 30000)]
    public float brakeForce = 16000;
    [Range(0.001f, 0.5f)]
    public float steerRateLerp = 0.1f;
    [Range(0, 1)]
    public float baseExtremiumSlip = 0.3f;
    public Vector3 addedDownforce;

    //direction is -1 for left and +1 for right, 0 for center
    void IDrivable.Steer(float targetDirection) {
        float targetAngle;
        float steerAngle;

        //Get the current steer angle
        steerAngle = frontLeftW.steerAngle;

        //targetAngle is the angle we want to tend towards
        targetAngle = targetDirection * maxSteerAngle;

        steerAngle = Mathf.Lerp(steerAngle, targetAngle, steerRateLerp);


        //set the steer angle
        frontLeftW.steerAngle = steerAngle;
        frontRightW.steerAngle = steerAngle;

        float extremiumSlip;
        WheelFrictionCurve flC = frontLeftW.sidewaysFriction;
        WheelFrictionCurve frC = frontRightW.sidewaysFriction;
        WheelFrictionCurve rl1C = rearLeft1W.sidewaysFriction;
        WheelFrictionCurve rr1C = rearRight1W.sidewaysFriction; 
        WheelFrictionCurve rl2C = rearLeft2W.sidewaysFriction;
        WheelFrictionCurve rr2C = rearRight2W.sidewaysFriction;

        extremiumSlip = baseExtremiumSlip + Mathf.Abs(steerAngle / maxSteerAngle);
        flC.extremumSlip = extremiumSlip;
        frC.extremumSlip = extremiumSlip;
        rl1C.extremumSlip = extremiumSlip;
        rr1C.extremumSlip = extremiumSlip;
        rl2C.extremumSlip = extremiumSlip;
        rr2C.extremumSlip = extremiumSlip;

        frontLeftW.sidewaysFriction = flC;
        frontRightW.sidewaysFriction = frC;
        rearLeft1W.sidewaysFriction = rl1C;
        rearRight1W.sidewaysFriction = rr1C;
        rearLeft2W.sidewaysFriction = rl2C;
        rearRight2W.sidewaysFriction = rr2C;


    }

    void IDrivable.Drift() {
        WheelFrictionCurve flC = frontLeftW.sidewaysFriction;
        WheelFrictionCurve frC = frontRightW.sidewaysFriction;
        WheelFrictionCurve rl1C = rearLeft1W.sidewaysFriction;
        WheelFrictionCurve rr1C = rearRight1W.sidewaysFriction;
        WheelFrictionCurve rl2C = rearLeft2W.sidewaysFriction;
        WheelFrictionCurve rr2C = rearRight2W.sidewaysFriction;

        float stiffness = 1f;
        flC.stiffness = stiffness;
        frC.stiffness = stiffness;
        rl1C.stiffness = stiffness;
        rr1C.stiffness = stiffness;
        rl2C.stiffness = stiffness;
        rr2C.stiffness = stiffness;

        frontLeftW.sidewaysFriction = flC;
        frontRightW.sidewaysFriction = frC;
        rearLeft1W.sidewaysFriction = rl1C;
        rearRight1W.sidewaysFriction = rr1C;
        rearLeft2W.sidewaysFriction = rl2C;
        rearRight2W.sidewaysFriction = rr2C;
    }

    void IDrivable.StopDrift() {
        WheelFrictionCurve flC = frontLeftW.sidewaysFriction;
        WheelFrictionCurve frC = frontRightW.sidewaysFriction;
        WheelFrictionCurve rl1C = rearLeft1W.sidewaysFriction;
        WheelFrictionCurve rr1C = rearRight1W.sidewaysFriction;
        WheelFrictionCurve rl2C = rearLeft2W.sidewaysFriction;
        WheelFrictionCurve rr2C = rearRight2W.sidewaysFriction;

        float stiffness = 5f;
        flC.stiffness = stiffness;
        frC.stiffness = stiffness;
        rl1C.stiffness = stiffness;
        rr1C.stiffness = stiffness;
        rl2C.stiffness = stiffness;
        rr2C.stiffness = stiffness;

        frontLeftW.sidewaysFriction = flC;
        frontRightW.sidewaysFriction = frC;
        rearLeft1W.sidewaysFriction = rl1C;
        rearRight1W.sidewaysFriction = rr1C;
        rearLeft2W.sidewaysFriction = rl2C;
        rearRight2W.sidewaysFriction = rr2C;
    }

    void IDrivable.Accellerate() {
        //check if needing to brake or accellerate
        rearRight2W.motorTorque = motorTorque;
        if (transform.InverseTransformDirection(carRB.velocity).z > -4) {
            ((IDrivable)this).StopBrake();
            frontLeftW.motorTorque = motorTorque;
            frontRightW.motorTorque = motorTorque;
            rearLeft1W.motorTorque = motorTorque;
            rearLeft2W.motorTorque = motorTorque;
            rearRight1W.motorTorque = motorTorque;
            rearRight2W.motorTorque = motorTorque;

        } else {
            ((IDrivable)this).Brake();
        }

    }
    void IDrivable.Reverse() {
        //check if needing to reverse or brake first
        if (transform.InverseTransformDirection(carRB.velocity).z < 4) {
            ((IDrivable)this).StopBrake();
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
        frontLeftW.brakeTorque = brakeTorque;
        frontRightW.brakeTorque = brakeTorque;
        rearLeft1W.brakeTorque = brakeTorque;
        rearLeft2W.brakeTorque = brakeTorque;
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
        UpdateWheelPose(frontLeftW, frontLeftT, true);
        UpdateWheelPose(frontRightW, frontRightT, false);
        UpdateWheelPose(rearLeft1W, rearLeft1T, true);
        UpdateWheelPose(rearLeft2W, rearLeft2T, true);
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
        frontLeftW.motorTorque = 0;
        frontRightW.motorTorque = 0;
        rearLeft1W.motorTorque = 0;
        rearLeft2W.motorTorque = 0;
        rearRight1W.motorTorque = 0;
        rearRight2W.motorTorque = 0;

    }

    void IDrivable.StopBrake() {
        frontLeftW.brakeTorque = 0;
        frontRightW.brakeTorque = 0;
        rearLeft1W.brakeTorque = 0;
        rearLeft2W.brakeTorque = 0;
        rearRight1W.brakeTorque = 0;
        rearRight2W.brakeTorque = 0;

    }

    void IDrivable.StopSteer() {
        //steer towards 0
        ((IDrivable)this).Steer(0);
    }
}
