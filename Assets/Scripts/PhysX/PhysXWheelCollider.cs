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
    public float suspensionSpringStrength = 10000f;
    public float suspensionSpringDamper = 5000f;
    public float suspensionSpringTargetPosition = 0.5f;
    public float extremumForwardSlip = 1;
    public float extremumForwardFriction = 1;
    public float asymptoteForwardSlip = 2;
    public float asymptoteForwardFriction = 0.5f;
    public float forwardStiffness = 1;
    public float asymptoteSidewaysTireLoad = 2;
    public float asymptoteSidewaysStiffness = 1;

    [HideInInspector]
    public float brakeTorque = 0;
    [HideInInspector]
    public bool isGrounded = true;
    [HideInInspector]
    public float motorTorque = 0;
    [HideInInspector]
    public float steerAngle = 0;

    public Vector3 wheelCentre { get; private set; }

    private IntPtr wheel = IntPtr.Zero;
    private IntPtr tire = IntPtr.Zero;
    private IntPtr suspension = IntPtr.Zero;

    private IntPtr vehicle = IntPtr.Zero;

    public IntPtr SetupInitialProperties() {
        wheelCentre = transform.position + transform.up * -suspensionDistance * suspensionSpringTargetPosition;

        wheel = PhysXLib.CreateWheelData();
        PhysXLib.SetWheelMass(wheel, mass);
        PhysXLib.SetWheelRadius(wheel, radius);
        PhysXLib.SetWheelDampingRate(wheel, dampingRate);
        PhysXLib.SetWheelMomentOfInertia(wheel, 0.5f * mass * radius * radius);
        PhysXLib.SetWheelWidth(wheel, width);

        tire = PhysXLib.CreateTireData();
        PhysXLib.SetTireBaseFriction(tire, 0);
        PhysXLib.SetTireMaxFrictionSlipPoint(tire, extremumForwardSlip);
        PhysXLib.SetTireMaxFriction(tire, extremumForwardFriction);
        PhysXLib.SetTirePlateuxSlipPoint(tire, asymptoteForwardSlip);
        PhysXLib.SetTirePlateuxFriction(tire, asymptoteForwardFriction);
        PhysXLib.SetTireLongitudinalStiffnessScale(tire, forwardStiffness);
        PhysXLib.SetTireLateralStiffnessMaxLoad(tire, asymptoteSidewaysTireLoad);
        PhysXLib.SetTireMaxLateralStiffness(tire, asymptoteSidewaysStiffness);

        suspension = PhysXLib.CreateSuspensionData();
        PhysXLib.SetSuspensionMaxCompression(suspension, suspensionDistance * suspensionSpringTargetPosition);
        PhysXLib.SetSuspensionMaxDroop(suspension, suspensionDistance * (1 - suspensionSpringTargetPosition));
        PhysXLib.SetSuspensionSpringStrength(suspension, suspensionSpringStrength);
        PhysXLib.SetSuspensionSpringDamper(suspension, suspensionSpringDamper);

        return suspension;
    }

    public void SetupSimData(IntPtr wheelSimData, int wheelNum) {
        PhysXLib.SetWheelSimForceAppPoint(wheelSimData, wheelNum, new PhysXVec3(wheelCentre + forceAppPoint));
        PhysXLib.SetWheelSimWheelCentre(wheelSimData, wheelNum, new PhysXVec3(wheelCentre));
        PhysXLib.SetWheelSimWheelData(wheelSimData, wheelNum, wheel);
        PhysXLib.SetWheelSimTireData(wheelSimData, wheelNum, tire);
        PhysXLib.SetWheelSimSuspensionData(wheelSimData, wheelNum, suspension, new PhysXVec3(-transform.up));
        PhysXLib.SetWheelSimWheelShape(wheelSimData, wheelNum, -1);
    }

    public void SetVehicle(IntPtr vehicle) {
        this.vehicle = vehicle;
    }
}
