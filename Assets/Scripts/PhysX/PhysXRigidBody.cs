using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXRigidBody : PhysXBody
{
    private List<PhysXWheelCollider> wheels;

    //public IntPtr physXBody { get; private set; }
    private IntPtr vehicle = IntPtr.Zero;

    public bool kinematic = false;
    public float mass = 1;
    public float linearDamping = 0;
    public float angularDamping = 0;

    private Vector3 _velocity;
    public Vector3 velocity {
        get {
            return _velocity;
        }
    }

    private Vector3 _angularVelocity;
    public Vector3 angularVelocity {
        get {
            return _angularVelocity;
        }
    }

    [SerializeField]
    private Vector3 _centreOfMass = Vector3.zero;
    public Vector3 centreOfMass {
        get {
            return _centreOfMass;
        }
        set {
            _centreOfMass = value;

            PhysXVec3 position = new PhysXVec3(_centreOfMass);
            PhysXQuat rotation = new PhysXQuat(Quaternion.identity);

            IntPtr oldCentre = PhysXLib.GetCentreOfMass(physXBody);

            IntPtr newCentre = PhysXLib.CreateTransform(position, rotation);
            PhysXLib.SetRigidBodyMassPose(physXBody, newCentre);

            if (vehicle != IntPtr.Zero) PhysXLib.UpdateVehicleCentreOfMass(oldCentre, newCentre, vehicle);
        }
    }

    void Awake() {
        collisionEnterEvents = new List<ICollisionEnterEvent>(GetComponentsInChildren<ICollisionEnterEvent>(true));
        collisionStayEvents = new List<ICollisionStayEvent>(GetComponentsInChildren<ICollisionStayEvent>(true));
        collisionExitEvents = new List<ICollisionExitEvent>(GetComponentsInChildren<ICollisionExitEvent>(true));

        triggerEnterEvents = new List<ITriggerEnterEvent>(GetComponentsInChildren<ITriggerEnterEvent>(true));
        triggerExitEvents = new List<ITriggerExitEvent>(GetComponentsInChildren<ITriggerExitEvent>(true));

        sceneManager = FindObjectOfType<PhysXSceneManager>();

        sceneManager.AddActor(this);
    }

    public override void Setup() {
        IntPtr physXTransform = PhysXLib.CreateTransform(new PhysXVec3(transform.position), new PhysXQuat(transform.rotation));
        physXBody = PhysXLib.CreateDynamicRigidBody(physXTransform);

        // PhysXLib.RegisterCollisionEnterCallback(ProcessCollisionEnterEvents, physXDynamicRigidBody);
        // PhysXLib.RegisterCollisionStayCallback(ProcessCollisionStayEvents, physXDynamicRigidBody);
        // PhysXLib.RegisterCollisionExitCallback(ProcessCollisionExitEvents, physXDynamicRigidBody);

        PhysXLib.SetRigidBodyFlag(physXBody, PhysXLib.PhysXRigidBodyFlag.eKINEMATIC, kinematic);
        if (kinematic) PhysXLib.SetRigidBodyDominanceGroup(physXBody, 1);

        PhysXLib.SetRigidBodyMaxDepenetrationVelocity(physXBody, 10);


        PhysXCollider[] colliders = GetComponentsInChildren<PhysXCollider>(true);

        foreach (PhysXCollider collider in colliders) {
            collider.Setup(this);
        }
        
        PhysXLib.SetRigidBodyMassAndInertia(physXBody, mass, new PhysXVec3(Vector3.zero));
        PhysXLib.SetRigidBodyDamping(physXBody, linearDamping, angularDamping);
    }

    public override void PostSceneInsertionSetup() {
        wheels = new List<PhysXWheelCollider>(GetComponentsInChildren<PhysXWheelCollider>(true));

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
                wheels[i].SetupSimData(wheelSimData, i);
            }

            vehicle = PhysXLib.CreateVehicleFromRigidBody(physXBody, wheelSimData);

            for (int i = 0; i < wheels.Count; i++) {
                wheels[i].SetVehicle(vehicle);
            }
        }

        PhysXVec3 position = new PhysXVec3(centreOfMass);
        PhysXQuat rotation = new PhysXQuat(Quaternion.identity);

        IntPtr oldCentre = PhysXLib.GetCentreOfMass(physXBody);

        IntPtr newCentre = PhysXLib.CreateTransform(position, rotation);
        PhysXLib.SetRigidBodyMassPose(physXBody, newCentre);

        if (vehicle != IntPtr.Zero) PhysXLib.UpdateVehicleCentreOfMass(oldCentre, newCentre, vehicle);
    }

    public override void UpdatePositionAndVelocity() {
        PhysXVec3 p = new PhysXVec3(Vector3.zero);
        PhysXLib.GetPosition(physXBody, p);

        PhysXQuat q = new PhysXQuat(Quaternion.identity);
        PhysXLib.GetRotation(physXBody, q);

        p.ToVector(ref _position);
        q.ToQuaternion(ref _rotation);

        transform.SetPositionAndRotation(_position, _rotation);

        PhysXVec3 lv = new PhysXVec3(Vector3.zero);
        PhysXLib.GetLinearVelocity(physXBody, lv);
        lv.ToVector(ref _velocity);

        PhysXVec3 av = new PhysXVec3(Vector3.zero);
        PhysXLib.GetAngularVelocity(physXBody, av);
        av.ToVector(ref _angularVelocity);

        foreach (PhysXWheelCollider wheel in wheels) {
            wheel.UpdateData();
        }
    }

    public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        PhysXLib.AddForce(physXBody, new PhysXVec3(force), forceModeInt);
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode forceMode) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        PhysXLib.AddForceAtPosition(physXBody, new PhysXVec3(force), new PhysXVec3(position), forceModeInt);
    }

    public void AddTorque(Vector3 force, ForceMode forceMode) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        PhysXLib.AddTorque(physXBody, new PhysXVec3(force), forceModeInt);
    }
}
