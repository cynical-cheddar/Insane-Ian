using PhysX;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceCarDrive4W : InterfaceCarDrive, IDrivable {
    public float maxSpeed = 30f;

    [Header("Wheel Colliders:")]
    public PhysXWheelCollider frontLeftW;
    public PhysXWheelCollider frontRightW;
    public PhysXWheelCollider rearLeftW;
    public PhysXWheelCollider rearRightW;
    public bool is4WD = true;
    [Space(5)]



    [Header("Wheel Geometry Transforms")]
    public Transform frontLeftT;
    public Transform frontRightT;
    public Transform rearLeftT;
    public Transform rearRightT;
    [Space(5)]

    [Header("Main Car")]
    public PhysXRigidBody carRB;
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
    [Range(0, 20000)]
    public float antiRollStiffness = 5000;
    [Range(0, 5)]
    public float baseStiffness = 2f;
    [Range(0, 2)]
    public float driftStiffness = 0.3f;

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

    [Space(5)]
    [Header("Additional parameters")]
    public bool isDead = false;
    private List<wheelStruct> wheelStructs = new List<wheelStruct>();

    struct wheelStruct {
        public float groundStiffness;
        public string surface;
        public PhysXWheelCollider collider;

        public wheelStruct(float groundStiffness, string surface, PhysXWheelCollider wc) {
            this.groundStiffness = groundStiffness;
            this.surface = surface;
            this.collider = wc;
        }

    }

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

    }
    void IDrivable.Accellerate() {
        //check if needing to brake or accellerate
        if (transform.InverseTransformDirection(carRB.velocity).z > -4) {
            ((IDrivable)this).StopBrake();
            if (carRB.velocity.magnitude < maxSpeed) {
                rearLeftW.motorTorque = motorTorque;
                rearRightW.motorTorque = motorTorque;
                if (is4WD) {
                    frontLeftW.motorTorque = motorTorque;
                    frontRightW.motorTorque = motorTorque;
                }
            } else {
                rearLeftW.motorTorque = 0;
                rearRightW.motorTorque = 0;
                if (is4WD) {
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
        foreach (wheelStruct ws in wheelStructs) {
            ws.collider.brakeTorque = brakeTorque;
        }

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
        foreach (wheelStruct ws in wheelStructs) {
            ws.collider.asymptoteSidewaysStiffness = ws.groundStiffness * driftStiffness;
        }
    }
    void IDrivable.StopDrift() {
        foreach (wheelStruct ws in wheelStructs) {
            ws.collider.asymptoteSidewaysStiffness = ws.groundStiffness * baseStiffness;
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
    void IDrivable.StopAccellerate() {
        foreach (wheelStruct ws in wheelStructs) {
            ws.collider.motorTorque = 0;
        }


    }
    void IDrivable.StopBrake() {
        foreach (wheelStruct ws in wheelStructs) {
            ws.collider.brakeTorque = 0;
        }

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
            EngineHigh.volume = Mathf.Lerp(EngineHigh.volume, volume / 10, 0.1f);
            EngineLow.volume = 1 - EngineHigh.volume;
        }

        EngineLow.pitch = 2.4f + volume / 10;
        EngineHigh.pitch = 3f + volume / 10;
    }

    private void AntiRoll(PhysXWheelCollider left, PhysXWheelCollider right) {

        PhysXWheelHit lHit = PhysXWheelHit.GetWheelHit();
        PhysXWheelHit rHit = PhysXWheelHit.GetWheelHit();
        bool lGrounded = left.GetGroundHit(lHit);
        bool rGrounded = right.GetGroundHit(rHit);
        float lDistance = 1f;
        float rDistance = 1f;

        if (lGrounded) {
            lDistance = (-left.transform.InverseTransformPoint(lHit.point).y - left.radius) / left.suspensionDistance;
        }

        if (rGrounded) {
            rDistance = (-right.transform.InverseTransformPoint(rHit.point).y - right.radius) / right.suspensionDistance;
        }

        float addedForce = (lDistance - rDistance) * antiRollStiffness;

        if (lGrounded) {
            carRB.AddForceAtPosition(left.transform.up * -addedForce, left.transform.position, ForceMode.Force);
        }

        if (rGrounded) {
            carRB.AddForceAtPosition(right.transform.up * addedForce, right.transform.position, ForceMode.Force);

        }
    }
    private void Particles() {
        PhysXWheelHit lHit = PhysXWheelHit.GetWheelHit();
        PhysXWheelHit rHit = PhysXWheelHit.GetWheelHit();
        bool lGrounded = rearLeftW.GetGroundHit(lHit);
        bool rGrounded = rearRightW.GetGroundHit(rHit);
        var lEmission = leftPS.emission;
        var rEmission = rightPS.emission;

        // left rear dust emission
        if (lGrounded && (Mathf.Abs(rearLeftW.rpm) > 150 || carRB.velocity.magnitude > 5)) {
            if (lHit.collider.CompareTag("DustGround")) {
                lEmission.enabled = true;
            } else {
                lEmission.enabled = false;
            }
        } else {
            lEmission.enabled = false;
        }

        // right rear dust emission
        if (rGrounded && (Mathf.Abs(rearRightW.rpm) > 150 || carRB.velocity.magnitude > 5)) {
            if (rHit.collider.CompareTag("DustGround")) {
                rEmission.enabled = true;
            } else {
                rEmission.enabled = false;
            }
        } else {
            rEmission.enabled = false;
        }

        PhysXWheelHit.ReleaseWheelHit(lHit);
        PhysXWheelHit.ReleaseWheelHit(rHit);
    }

    private void getSurface() {
        for (int i = 0; i < wheelStructs.Count; i++) {
            PhysXWheelHit hit = PhysXWheelHit.GetWheelHit();
            if (wheelStructs[i].collider.GetGroundHit(hit)) { //for each wheel
                // if new ground type, set new stiffness
                if (hit.collider.CompareTag("DustGround") && wheelStructs[i].surface != "DustGround") {
                    wheelStructs[i] = new wheelStruct(5f, "DustGround", wheelStructs[i].collider);
                } else {
                    wheelStructs[i] = new wheelStruct(8f, "0", wheelStructs[i].collider);
                }
            }
        }
    }
    private void AutoRight() {
        float angle = Mathf.Abs(Vector3.Angle(transform.up, Vector3.up));

        // if tipping by at least 45 degrees, nudge back
        if (angle > 45 && !isDead) {
            if (angle > 120) {
                // at severe angles, offset center of mass from center so if stuck on roof, can rotate over
                carRB.centreOfMass = new Vector3(-1f, -3f, 0);
            } else {
                carRB.centreOfMass = new Vector3(0, -2.5f, 0);
            }
        } else if (!isDead)
            carRB.centreOfMass = new Vector3(0, 0, 0);

    }

    void FixedUpdate() {
        getSurface();
        EngineNoise();
        AntiRoll(frontLeftW, frontRightW);
        AntiRoll(rearLeftW, rearRightW);
        AutoRight();
        Particles();
    }


    private void Start() {
        EngineIdle.volume = 0;
        EngineLow.volume = 0;
        EngineHigh.volume = 0;

        wheelStructs.Add(new wheelStruct(1f, "", frontLeftW));
        wheelStructs.Add(new wheelStruct(1f, "", frontRightW));
        wheelStructs.Add(new wheelStruct(1f, "", rearLeftW));
        wheelStructs.Add(new wheelStruct(1f, "", rearRightW));
    }
}

