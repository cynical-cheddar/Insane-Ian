using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXRigidBody : MonoBehaviour
{
    private Dictionary<IntPtr, PhysXCollider> colliders = new Dictionary<IntPtr, PhysXCollider>();

    private bool isSetup = false;

    public IntPtr physXDynamicRigidBody { get; private set; }

    private PhysXSceneManager sceneManager;

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

    private Vector3 _position;
    public Vector3 position {
        get {
            return _position;
        }
    }

    private Quaternion _rotation;
    public Quaternion rotation {
        get {
            return _rotation;
        }
    }

    public Vector3 centreOfMass = Vector3.zero;

    private List<ICollisionEnterEvent> collisionEnterEvents;

    void Awake() {
        collisionEnterEvents = new List<ICollisionEnterEvent>(GetComponentsInChildren<ICollisionEnterEvent>(true));

        sceneManager = FindObjectOfType<PhysXSceneManager>();

        sceneManager.AddActor(this);
    }

    public void Setup() {
        IntPtr physXTransform = PhysXLib.CreateTransform(new PhysXVec3(transform.position), new PhysXQuat(transform.rotation));
        physXDynamicRigidBody = PhysXLib.CreateDynamicRigidBody(physXTransform);

        // PhysXLib.RegisterCollisionEnterCallback(ProcessCollisionEnterEvents, physXDynamicRigidBody);
        // PhysXLib.RegisterCollisionStayCallback(ProcessCollisionStayEvents, physXDynamicRigidBody);
        // PhysXLib.RegisterCollisionExitCallback(ProcessCollisionExitEvents, physXDynamicRigidBody);

        PhysXLib.SetRigidBodyFlag(physXDynamicRigidBody, PhysXLib.PhysXRigidBodyFlag.eKINEMATIC, kinematic);

        PhysXLib.SetRigidBodyMassAndInertia(physXDynamicRigidBody, mass, new PhysXVec3(centreOfMass));
        PhysXLib.SetRigidBodyDamping(physXDynamicRigidBody, linearDamping, angularDamping);

        PhysXCollider[] colliders = GetComponentsInChildren<PhysXCollider>(true);

        foreach (PhysXCollider collider in colliders) {
            collider.Setup();
        }
    }

    public void AddCollider(PhysXCollider collider) {
        colliders.Add(collider.shape, collider);
        PhysXLib.AttachShapeToRigidBody(collider.shape, physXDynamicRigidBody);
    }

    public PhysXCollider GetColliderFromShape(IntPtr shape) {
        return colliders[shape];
    }

    public void UpdatePositionAndVelocity() {
        PhysXVec3 p = new PhysXVec3(Vector3.zero);
        PhysXLib.GetPosition(physXDynamicRigidBody, p);

        PhysXQuat q = new PhysXQuat(Quaternion.identity);
        PhysXLib.GetRotation(physXDynamicRigidBody, q);

        p.ToVector(ref _position);
        q.ToQuaternion(ref _rotation);

        transform.SetPositionAndRotation(_position, _rotation);

        PhysXVec3 lv = new PhysXVec3(Vector3.zero);
        PhysXLib.GetLinearVelocity(physXDynamicRigidBody, lv);
        lv.ToVector(ref _velocity);

        PhysXVec3 av = new PhysXVec3(Vector3.zero);
        PhysXLib.GetAngularVelocity(physXDynamicRigidBody, av);
        av.ToVector(ref _angularVelocity);
    }

    public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        PhysXLib.AddForce(physXDynamicRigidBody, new PhysXVec3(force), forceModeInt);
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode forceMode) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        PhysXLib.AddForceAtPosition(physXDynamicRigidBody, new PhysXVec3(force), new PhysXVec3(position), forceModeInt);
    }

    public void AddTorque(Vector3 force, ForceMode forceMode) {
        int forceModeInt = (int)forceMode;
        if (forceMode == ForceMode.Acceleration) forceModeInt = 3;

        PhysXLib.AddTorque(physXDynamicRigidBody, new PhysXVec3(force), forceModeInt);
    }

    public void FireCollisionEvents(PhysXCollision collision) {
        if (collision.isEnter) {
            foreach (ICollisionEnterEvent collisionEnterEvent in collisionEnterEvents) {
                if (collisionEnterEvent.requiresData) collisionEnterEvent.OnCollisionEnter(collision);
                else collisionEnterEvent.OnCollisionEnter();
            }
        }
    }
}
