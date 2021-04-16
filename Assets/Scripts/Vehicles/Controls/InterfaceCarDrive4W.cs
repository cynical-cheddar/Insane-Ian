using System.Collections.Generic;
using UnityEngine;

public class InterfaceCarDrive4W : InterfaceCarDrive, IDrivable {
    // Start is called before the first frame update

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
    [Range(0, 20000)]
    public float antiRollStiffness = 5000;
    [Range(0, 30)]
    public float baseStiffness = 15f;
    [Range(0, 20)]
    public float driftStiffness = 5f;

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
        Debug.LogWarning("Interface Car Drive has not been fully ported to the new PhysX system");
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
        Debug.LogWarning("Interface Car Drive has not been fully ported to the new PhysX system");
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

        Debug.LogWarning("Interface Car Drive has not been fully ported to the new PhysX system");
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
            //ws.collider.asymptoteSidewaysStiffness = ws.groundStiffness * driftStiffness;
            ws.collider.asymptoteSidewaysStiffness = driftStiffness;
        }
    }
    void IDrivable.StopDrift() {
        foreach (wheelStruct ws in wheelStructs) {
            //ws.collider.asymptoteSidewaysStiffness = ws.groundStiffness * baseStiffness;
            ws.collider.asymptoteSidewaysStiffness = baseStiffness;
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
        /*
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
        EngineHigh.pitch = 2.4f + volume / 10;
        */
    }

    private void AntiRoll(PhysXWheelCollider left, PhysXWheelCollider right) {
        /*
       WheelHit lHit, rHit;
       float lDistance = 1f;
       float rDistance = 1f;

       bool lGrounded = left.GetGroundHit(out lHit);
       bool rGrounded = right.GetGroundHit(out rHit);

       //  Can get suspension compression if tht's useful
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

       } */
    }
    private void Particles() {
        /*
        WheelHit lHit, rHit;
        bool lGrounded = rearLeftW.GetGroundHit(out lHit);
        bool rGrounded = rearRightW.GetGroundHit(out rHit);
        var lEmission = leftPS.emission;
        var rEmission = rightPS.emission;

        if (lGrounded && (Mathf.Abs(rearLeftW.rpm) > 150 || carRB.velocity.magnitude > 5)) {
            if (lHit.collider.CompareTag("DustGround")) {
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
        */
    }

    private void getSurface() {
        /*
        for (int i = 0; i < wheelStructs.Count; i++) { 
            WheelHit hit;
            wheelStructs[i].collider.GetGroundHit(out hit);
            if (hit.collider != null) {
                if (hit.collider.CompareTag("DustGround") && wheelStructs[i].surface != "DustGround") {
                    wheelStructs[i] = new wheelStruct(5f, "DustGround", wheelStructs[i].collider);
                } else {
                    wheelStructs[i] = new wheelStruct(8f, "0", wheelStructs[i].collider);
                }
            }
        } */
    }

    void FixedUpdate() {
        getSurface();
        EngineNoise();
        AntiRoll(frontLeftW, frontRightW);
        AntiRoll(rearLeftW, rearRightW);
        Particles();
    }



    private void Start() {
        EngineIdle.volume = 0;
        EngineLow.volume = 0;
        EngineHigh.volume = 0;

        wheelStructs.Add(new wheelStruct(0f, "", frontLeftW));
        wheelStructs.Add(new wheelStruct(0f, "", frontRightW));
        wheelStructs.Add(new wheelStruct(0f, "", rearLeftW));
        wheelStructs.Add(new wheelStruct(0f, "", rearRightW));


    }
}

