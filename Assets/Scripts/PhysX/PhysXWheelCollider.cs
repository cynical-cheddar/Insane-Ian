using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXWheelCollider : MonoBehaviour
{
    public float mass = 20;
    public float radius = 0.2f;
    public float width = 0.2f;
    public float dampingRate = 0.1f;
    public Vector3 forceAppPoint = Vector3.up * 0.1f;
    public Vector3 center = Vector3.zero;
    public float suspensionDistance = 0.2f;
    public float suspensionSpringStrength = 10000;
    public float suspensionSpringDamper = 5000;
    public float suspensionSpringTargetPosition = 0.5f;
    public float baseForwardFriction = 5;
    public float extremumForwardSlip = 1;
    public float extremumForwardFriction = 10;
    public float asymptoteForwardSlip = 2;
    public float asymptoteForwardFriction = 5;
    public float forwardStiffness = 1000;
    public float asymptoteSidewaysTireLoad = 2;

    [SerializeField]
    private float _asymptoteSidewaysStiffness = 10;
    public float asymptoteSidewaysStiffness {
        get {
            return _asymptoteSidewaysStiffness;
        }
        set {
            _asymptoteSidewaysStiffness = value;

            PhysXLib.SetTireMaxLateralStiffness(tire, _asymptoteSidewaysStiffness);
            PhysXLib.SetWheelSimTireData(wheelSimData, wheelNum, tire);
        }
    }

    private float _brakeTorque = 0;
    [HideInInspector]
    public float brakeTorque {
        get {
            return _brakeTorque;
        }
        set {
            _brakeTorque = value;

            PhysXLib.SetWheelBrake(vehicle, wheelNum, _brakeTorque);
        }
    }

    [HideInInspector]
    public bool isGrounded = true;

    private float _motorTorque = 0;
    [HideInInspector]
    public float motorTorque {
        get {
            return _motorTorque;
        }
        set {
            _motorTorque = value;

            PhysXLib.SetWheelDrive(vehicle, wheelNum, _motorTorque);
        }
    }

    private float _steerAngle = 0;
    [HideInInspector]
    public float steerAngle {
        get {
            return _steerAngle;
        }
        set {
            _steerAngle = value;

            PhysXLib.SetWheelSteer(vehicle, wheelNum, _steerAngle);
        }
    }

    public Vector3 wheelCentre { get; private set; }
    public Vector3 worldWheelCentre {
        get {
            return transform.TransformPoint(wheelCentre);
        }
    }

    private IntPtr wheel = IntPtr.Zero;
    private IntPtr tire = IntPtr.Zero;
    private IntPtr suspension = IntPtr.Zero;

    private IntPtr vehicle = IntPtr.Zero;
    private IntPtr wheelSimData = IntPtr.Zero;
    private int wheelNum = 0;

    public IntPtr SetupInitialProperties() {
        wheelCentre = Vector3.up * -suspensionDistance * suspensionSpringTargetPosition;

        wheel = PhysXLib.CreateWheelData();
        PhysXLib.SetWheelMass(wheel, mass);
        PhysXLib.SetWheelRadius(wheel, radius);
        PhysXLib.SetWheelDampingRate(wheel, dampingRate);
        PhysXLib.SetWheelMomentOfInertia(wheel, 0.5f * mass * radius * radius);
        PhysXLib.SetWheelWidth(wheel, width);

        tire = PhysXLib.CreateTireData();
        PhysXLib.SetTireBaseFriction(tire, baseForwardFriction);
        PhysXLib.SetTireMaxFrictionSlipPoint(tire, extremumForwardSlip);
        PhysXLib.SetTireMaxFriction(tire, extremumForwardFriction);
        PhysXLib.SetTirePlateuxSlipPoint(tire, asymptoteForwardSlip);
        PhysXLib.SetTirePlateuxFriction(tire, asymptoteForwardFriction);
        PhysXLib.SetTireLongitudinalStiffnessScale(tire, forwardStiffness);
        PhysXLib.SetTireLateralStiffnessMaxLoad(tire, asymptoteSidewaysTireLoad);
        PhysXLib.SetTireMaxLateralStiffness(tire, _asymptoteSidewaysStiffness);

        suspension = PhysXLib.CreateSuspensionData();
        PhysXLib.SetSuspensionMaxCompression(suspension, suspensionDistance * suspensionSpringTargetPosition);
        PhysXLib.SetSuspensionMaxDroop(suspension, suspensionDistance * (1 - suspensionSpringTargetPosition));
        PhysXLib.SetSuspensionSpringStrength(suspension, suspensionSpringStrength);
        PhysXLib.SetSuspensionSpringDamper(suspension, suspensionSpringDamper);

        return suspension;
    }

    public void SetupSimData(IntPtr wheelSimData, int wheelNum) {
        this.wheelNum = wheelNum;

        Transform grandestParent = transform;
        while (grandestParent.parent != null) {
            grandestParent = grandestParent.parent;
        }

        PhysXVec3 wheelCentrePos = new PhysXVec3(grandestParent.InverseTransformPoint(transform.TransformPoint(wheelCentre)));
        PhysXVec3 forceAppPos = new PhysXVec3(grandestParent.InverseTransformPoint(transform.TransformPoint(wheelCentre + forceAppPoint)));
        //PhysXQuat rotation = new PhysXQuat(transform.rotation * Quaternion.Inverse(grandestParent.rotation));

        PhysXLib.SetWheelSimForceAppPoint(wheelSimData, wheelNum, forceAppPos);
        PhysXLib.SetWheelSimWheelCentre(wheelSimData, wheelNum, wheelCentrePos);
        PhysXLib.SetWheelSimWheelData(wheelSimData, wheelNum, wheel);
        PhysXLib.SetWheelSimTireData(wheelSimData, wheelNum, tire);
        PhysXLib.SetWheelSimSuspensionData(wheelSimData, wheelNum, suspension, new PhysXVec3(-transform.up));
        PhysXLib.SetWheelSimWheelShape(wheelSimData, wheelNum, -1);
    }

    public void SetVehicle(IntPtr vehicle) {
        this.vehicle = vehicle;

        wheelSimData = PhysXLib.GetWheelSimData(vehicle);
    }

    public void UpdateData() {
        wheelCentre = Vector3.up * (PhysXLib.GetSuspensionCompression(vehicle, wheelNum) - suspensionDistance * suspensionSpringTargetPosition);
    }

    void OnDrawGizmos() {
        if (Application.isPlaying) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(wheelCentre), radius);

            Transform grandestParent = transform;
            while (grandestParent.parent != null) {
                grandestParent = grandestParent.parent;
            }
            PhysXVec3 position = new PhysXVec3(Vector3.zero);
            PhysXQuat rotation = new PhysXQuat(Quaternion.identity);
            PhysXLib.GetWheelTransform(vehicle, wheelNum, position, rotation);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grandestParent.TransformPoint(position.ToVector()), radius);
        }
        else {
            
        }
    }

    public void GetWorldPose(out Vector3 position, out Quaternion rotation) {
        position = transform.TransformPoint(wheelCentre);
        rotation = Quaternion.AngleAxis(steerAngle * Mathf.Rad2Deg, transform.TransformDirection(transform.up));
    }
}
