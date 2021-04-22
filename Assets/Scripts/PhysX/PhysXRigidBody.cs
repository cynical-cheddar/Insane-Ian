using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXRigidBody : PhysXBody
{
    static uint currentVehicleId = 1;

    private List<PhysXWheelCollider> wheels;

    //public IntPtr physXBody { get; private set; }
    private IntPtr vehicle = IntPtr.Zero;

    public uint vehicleId { get; private set; } = 0;

    public bool kinematic = false;

    public bool useGravity = true;
    public float mass = 1;
    public float linearDamping = 0;
    public float angularDamping = 0;

    private PhysXVec3 physXVelocity = new PhysXVec3(Vector3.zero);
    private Vector3 _velocity;
    public Vector3 velocity {
        get {
            return _velocity;
        }
        set {
            _velocity = value;
            physXVelocity.FromVector(_velocity);
            PhysXLib.SetLinearVelocity(physXBody, physXVelocity);
        }
    }

    private PhysXVec3 physXAngularVelocity = new PhysXVec3(Vector3.zero);
    private Vector3 _angularVelocity;
    public Vector3 angularVelocity {
        get {
            return _angularVelocity;
        }
        set {
            _angularVelocity = value;
            physXVelocity.FromVector(_angularVelocity);
            PhysXLib.SetAngularVelocity(physXBody, physXAngularVelocity);
        }
    }

    private PhysXVec3 physXCOMPosition = new PhysXVec3(Vector3.zero);
    private PhysXQuat physXCOMRotation = new PhysXQuat(Quaternion.identity);
    [SerializeField]
    private Vector3 _centreOfMass = Vector3.zero;
    public Vector3 centreOfMass {
        get {
            return _centreOfMass;
        }
        set {
            _centreOfMass = value;

            physXCOMPosition.FromVector(_centreOfMass);

            IntPtr oldCentre = PhysXLib.GetCentreOfMass(physXBody);

            IntPtr newCentre = PhysXLib.CreateTransform(physXCOMPosition, physXCOMRotation);
            PhysXLib.SetRigidBodyMassPose(physXBody, newCentre);

            if (vehicle != IntPtr.Zero) PhysXLib.UpdateVehicleCentreOfMass(oldCentre, newCentre, vehicle);
        }
    }

    public override void Setup() {
        physXPosition.FromVector(transform.position);
        physXRotation.FromQuaternion(transform.rotation);
        IntPtr physXTransform = PhysXLib.CreateTransform(physXPosition, physXRotation);
        _position = transform.position;
        _rotation = transform.rotation;
        physXBody = PhysXLib.CreateDynamicRigidBody(physXTransform);

        // PhysXLib.RegisterCollisionEnterCallback(ProcessCollisionEnterEvents, physXDynamicRigidBody);
        // PhysXLib.RegisterCollisionStayCallback(ProcessCollisionStayEvents, physXDynamicRigidBody);
        // PhysXLib.RegisterCollisionExitCallback(ProcessCollisionExitEvents, physXDynamicRigidBody);

        PhysXLib.SetRigidBodyFlag(physXBody, PhysXLib.PhysXRigidBodyFlag.eKINEMATIC, kinematic);
        if (kinematic) PhysXLib.SetRigidBodyDominanceGroup(physXBody, 1);

        PhysXLib.SetRigidBodyMaxDepenetrationVelocity(physXBody, 10);


        wheels = new List<PhysXWheelCollider>(GetComponentsInChildren<PhysXWheelCollider>(true));
        PhysXCollider[] colliders = GetComponentsInChildren<PhysXCollider>(true);

        if (wheels.Count > 0) {
            vehicleId = currentVehicleId;
            currentVehicleId++;
        }
        //Debug.Log(vehicleId);

        foreach (PhysXCollider collider in colliders) {
            collider.Setup(this, vehicleId);
        }
        
        PhysXLib.SetRigidBodyMassAndInertia(physXBody, mass, new PhysXVec3(Vector3.zero));
        PhysXLib.SetRigidBodyDamping(physXBody, linearDamping, angularDamping);
    }

    public override void PostSceneInsertionSetup() {

        if (wheels.Count > 0) {
            IntPtr wheelSimData = PhysXLib.CreateWheelSimData(wheels.Count);
            IntPtr[] suspensions = new IntPtr[wheels.Count];
            IntPtr wheelPositions = PhysXLib.CreateVectorArray();

            for (int i = 0; i < wheels.Count; i++) {
                suspensions[i] = wheels[i].SetupInitialProperties();
                PhysXLib.AddVectorToArray(wheelPositions, new PhysXVec3(transform.InverseTransformPoint(wheels[i].worldWheelCentre)));
            }

            PhysXLib.SetSuspensionSprungMasses(suspensions, wheels.Count, wheelPositions, new PhysXVec3(Vector3.zero), mass);

            for (int i = 0; i < wheels.Count; i++) {
                //Debug.Log(vehicleId);
                wheels[i].SetupSimData(this, wheelSimData, i, vehicleId);
            }

            vehicle = PhysXLib.CreateVehicleFromRigidBody(physXBody, wheelSimData);

            for (int i = 0; i < wheels.Count; i++) {
                wheels[i].SetVehicle(vehicle);
            }
        }

        physXCOMPosition.FromVector(centreOfMass);
        // PhysXVec3 position = new PhysXVec3(centreOfMass);
        // PhysXQuat rotation = new PhysXQuat(Quaternion.identity);

        IntPtr oldCentre = PhysXLib.GetCentreOfMass(physXBody);

        IntPtr newCentre = PhysXLib.CreateTransform(physXCOMPosition, physXCOMRotation);
        PhysXLib.SetRigidBodyMassPose(physXBody, newCentre);

        if (vehicle != IntPtr.Zero) PhysXLib.UpdateVehicleCentreOfMass(oldCentre, newCentre, vehicle);
    }

    public override void UpdatePositionAndVelocity() {
        PhysXLib.GetPosition(physXBody, physXPosition);

        PhysXLib.GetRotation(physXBody, physXRotation);

        physXPosition.ToVector(ref _position);
        physXRotation.ToQuaternion(ref _rotation);

        transform.SetPositionAndRotation(_position, _rotation);

        PhysXLib.GetLinearVelocity(physXBody, physXVelocity);
        physXVelocity.ToVector(ref _velocity);

        PhysXLib.GetAngularVelocity(physXBody, physXAngularVelocity);
        physXAngularVelocity.ToVector(ref _angularVelocity);

        foreach (PhysXWheelCollider wheel in wheels) {
            wheel.UpdateData();
        }
    }

    private PhysXVec3 physXForce = new PhysXVec3(Vector3.zero);
    private PhysXVec3 physXForcePos = new PhysXVec3(Vector3.zero);
    public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        physXForce.FromVector(force);
        PhysXLib.AddForce(physXBody, physXForce, forceModeInt);
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode forceMode) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        physXForce.FromVector(force);
        physXForcePos.FromVector(position);
        PhysXLib.AddForceAtPosition(physXBody, physXForce, physXForcePos, forceModeInt);
    }

    public void AddTorque(Vector3 force, ForceMode forceMode) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        physXForce.FromVector(force);
        PhysXLib.AddTorque(physXBody, physXForce, forceModeInt);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if (vehicle != IntPtr.Zero) PhysXLib.DestroyVehicle(vehicle);
    }

    

    private void FixedUpdate() {
        if(!useGravity){
            AddForce(-Physics.gravity, ForceMode.Acceleration);
        }
    }
}
