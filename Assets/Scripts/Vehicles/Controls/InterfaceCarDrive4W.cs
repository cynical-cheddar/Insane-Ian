using System;
using UnityEngine;

public class InterfaceCarDrive4W : InterfaceCarDrive, IDrivable {
    // Start is called before the first frame update

    public float maxSpeed = 30f;

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
    [Range(1000, 80000)]
    public float motorTorque = 4500;
    [Range(2000, 80000)]
    public float brakeTorque = 8000;
    [Range(0, 30000)]
    public float brakeForce = 16000;
    [Range(0.001f, 0.5f)]
    public float steerRateLerp = 0.1f;
    [Range(0, 1)]
    public float baseExtremiumSlip = 0.3f;
    public Vector3 addedDownforce;
    [Range(0,20000)]
    public float antiRollStiffness = 5000;
    [Range(1, 7)]
    public float baseStiffness = 5;

    [Space(5)]

    [Header("Engine Noises")]
    public AudioSource EngineIdle;
    public AudioSource EngineLow;
    public AudioSource EngineHigh;
    private float volume = 0;
    [Space(5)]

    [Header("Dust Trail")]
    public ParticleSystem leftPS;
    public ParticleSystem rightPS;



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
        WheelFrictionCurve rlC = rearLeftW.sidewaysFriction;
        WheelFrictionCurve rrC = rearRightW.sidewaysFriction;

        extremiumSlip = baseExtremiumSlip + Mathf.Abs(steerAngle / maxSteerAngle);
        flC.extremumSlip = extremiumSlip;
        frC.extremumSlip = extremiumSlip;
        rlC.extremumSlip = extremiumSlip;
        rrC.extremumSlip = extremiumSlip;

        frontLeftW.sidewaysFriction = flC;
        frontRightW.sidewaysFriction = frC;
        rearLeftW.sidewaysFriction = rlC;
        rearRightW.sidewaysFriction = rrC;
    }
    void IDrivable.Accellerate() {
        //check if needing to brake or accellerate
        if (transform.InverseTransformDirection(carRB.velocity).z > -4) {
            ((IDrivable)this).StopBrake();
            if (carRB.velocity.magnitude < maxSpeed)
            {
                rearLeftW.motorTorque = motorTorque;
                rearRightW.motorTorque = motorTorque;
                if (is4WD)
                {
                    frontLeftW.motorTorque = motorTorque;
                    frontRightW.motorTorque = motorTorque;
                }
            }
            else
            {
                rearLeftW.motorTorque = 0;
                rearRightW.motorTorque = 0;
                if (is4WD)
                {
                    frontLeftW.motorTorque = 0;
                    frontRightW.motorTorque = 0;
                }
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
    void IDrivable.Drift() {
        WheelFrictionCurve flC = frontLeftW.sidewaysFriction;
        WheelFrictionCurve frC = frontRightW.sidewaysFriction;
        WheelFrictionCurve rlC = rearLeftW.sidewaysFriction;
        WheelFrictionCurve rrC = rearRightW.sidewaysFriction;

        float stiffness = 1f;
        flC.stiffness = stiffness;
        frC.stiffness = stiffness;
        rlC.stiffness = stiffness;
        rrC.stiffness = stiffness;

        frontLeftW.sidewaysFriction = flC;
        frontRightW.sidewaysFriction = frC;
        rearLeftW.sidewaysFriction = rlC;
        rearRightW.sidewaysFriction = rrC;
    }
    void IDrivable.StopDrift() {
        WheelFrictionCurve flC = frontLeftW.sidewaysFriction;
        WheelFrictionCurve frC = frontRightW.sidewaysFriction;
        WheelFrictionCurve rlC = rearLeftW.sidewaysFriction;
        WheelFrictionCurve rrC = rearRightW.sidewaysFriction;

        float stiffness = baseStiffness;
        flC.stiffness = stiffness;
        frC.stiffness = stiffness;
        rlC.stiffness = stiffness;
        rrC.stiffness = stiffness;

        frontLeftW.sidewaysFriction = flC;
        frontRightW.sidewaysFriction = frC;
        rearLeftW.sidewaysFriction = rlC;
        rearRightW.sidewaysFriction = rrC;
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
    
    private void EngineNoise() {
        float newpitch;
        newpitch = Mathf.Clamp((Mathf.Abs(frontLeftW.rpm + frontRightW.rpm + rearLeftW.rpm + rearRightW.rpm)) * 0.01f * 0.25f, 0, 14f);
        volume = Mathf.Lerp(volume, newpitch, 0.1f);
        if (volume < 1) {
            EngineIdle.volume = Mathf.Lerp(EngineIdle.volume, 1.0f, 0.1f);
            EngineLow.volume = Mathf.Lerp(EngineLow.volume, 0.3f, 0.1f);
            EngineHigh.volume = Mathf.Lerp(EngineHigh.volume, 0.0f, 0.1f);
        } else {
            EngineIdle.volume = Mathf.Lerp(EngineIdle.volume, 0f, 0.1f);
            EngineHigh.volume = Mathf.Lerp(EngineHigh.volume, volume/10, 0.1f);
            EngineLow.volume = 1 - EngineHigh.volume;
        }

        EngineLow.pitch = 2.4f + volume / 10;
        EngineHigh.pitch = 2.4f + volume / 10;

    }
    
    private void AntiRoll(WheelCollider left, WheelCollider right) {
        WheelHit lHit, rHit;
        float lDistance = 1f;
        float rDistance = 1f;

        bool lGrounded = left.GetGroundHit(out lHit);
        bool rGrounded = right.GetGroundHit(out rHit);

        if (lGrounded) {
            lDistance = (-left.transform.InverseTransformPoint(lHit.point).y - left.radius) / left.suspensionDistance;
        }

        if (rGrounded) {
            rDistance = (-right.transform.InverseTransformPoint(rHit.point).y - right.radius) / right.suspensionDistance;
        }

        float addedForce = (lDistance - rDistance) * antiRollStiffness;

        if (lGrounded) {
            carRB.AddForceAtPosition(left.transform.up * -addedForce, left.transform.position);
        }

        if (rGrounded) {
            carRB.AddForceAtPosition(right.transform.up * addedForce, right.transform.position);

        }
    }
    private void Particles() {
        WheelHit lHit, rHit;
        bool lGrounded = rearLeftW.GetGroundHit(out lHit);
        bool rGrounded = rearRightW.GetGroundHit(out rHit);
        var lEmission = leftPS.emission;
        var rEmission = rightPS.emission;

        if (lGrounded && (Mathf.Abs(rearLeftW.rpm) > 150 || carRB.velocity.magnitude > 5)) {
            if (lHit.collider.CompareTag("DustGround")){
                lEmission.enabled = true;
            } else {
                lEmission.enabled = false;
            }
        } else {
            lEmission.enabled = false;
        }
        if (rGrounded && (Mathf.Abs(rearRightW.rpm) > 150 || carRB.velocity.magnitude > 5)) {
            if (rHit.collider.CompareTag("DustGround")) {
                rEmission.enabled = true;
            } else {
                rEmission.enabled = false;
            }
        } else {
            rEmission.enabled = false;
        }
    }
    void FixedUpdate() {
        EngineNoise();
        AntiRoll(frontLeftW, frontRightW);
        AntiRoll(rearLeftW, rearRightW);
        Particles();
    }

    

    private void Start() {
        EngineIdle.volume = 0;
        EngineLow.volume = 0;
        EngineHigh.volume = 0;
    }
}

