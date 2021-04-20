using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXBody : MonoBehaviour
{
    protected Dictionary<IntPtr, PhysXCollider> colliders = new Dictionary<IntPtr, PhysXCollider>();

    protected bool isSetup = false;

    public IntPtr physXBody { get; protected set; }

    protected PhysXSceneManager sceneManager;

    protected PhysXVec3 physXPosition = new PhysXVec3(Vector3.zero);
    protected Vector3 _position;
    public Vector3 position {
        get {
            return _position;
        }
        set {
            _position = value;
            transform.position = _position;
            physXPosition.FromVector(_position);
            PhysXLib.SetPosition(physXBody, physXPosition);
        }
    }

    protected PhysXQuat physXRotation = new PhysXQuat(Quaternion.identity);
    protected Quaternion _rotation;
    public Quaternion rotation {
        get {
            return _rotation;
        }
        set {
            _rotation = value;
            transform.rotation = _rotation;
            physXRotation.FromQuaternion(_rotation);
            PhysXLib.SetRotation(physXBody, physXRotation);
        }
    }

    protected List<ICollisionEnterEvent> collisionEnterEvents;
    protected List<ICollisionStayEvent> collisionStayEvents;
    protected List<ICollisionExitEvent> collisionExitEvents;

    protected List<ITriggerEnterEvent> triggerEnterEvents;
    protected List<ITriggerExitEvent> triggerExitEvents;

    public PhysXLib.CollisionEvent collisionEventFlags {
        get {
            PhysXLib.CollisionEvent flags = 0;

            if (collisionEnterEvents.Count > 0) flags |= PhysXLib.CollisionEvent.CONTACT_BEGIN;
            if (collisionStayEvents.Count > 0) flags |= PhysXLib.CollisionEvent.CONTACT_SUSTAIN;
            if (collisionExitEvents.Count > 0) flags |= PhysXLib.CollisionEvent.CONTACT_END;

            if (triggerEnterEvents.Count > 0) flags |= PhysXLib.CollisionEvent.TRIGGER_BEGIN;
            if (triggerExitEvents.Count > 0) flags |= PhysXLib.CollisionEvent.TRIGGER_END;

            return flags;
        }
    }

    protected void Awake() {
        collisionEnterEvents = new List<ICollisionEnterEvent>(GetComponentsInChildren<ICollisionEnterEvent>(true));
        collisionStayEvents = new List<ICollisionStayEvent>(GetComponentsInChildren<ICollisionStayEvent>(true));
        collisionExitEvents = new List<ICollisionExitEvent>(GetComponentsInChildren<ICollisionExitEvent>(true));

        triggerEnterEvents = new List<ITriggerEnterEvent>(GetComponentsInChildren<ITriggerEnterEvent>(true));
        triggerExitEvents = new List<ITriggerExitEvent>(GetComponentsInChildren<ITriggerExitEvent>(true));

        sceneManager = FindObjectOfType<PhysXSceneManager>();

        sceneManager.AddActor(this);
    }

    public virtual void Setup() {
        physXPosition.FromVector(transform.position);
        physXRotation.FromQuaternion(transform.rotation);
        IntPtr physXTransform = PhysXLib.CreateTransform(physXPosition, physXRotation);
        _position = transform.position;
        _rotation = transform.rotation;
        physXBody = PhysXLib.CreateStaticRigidBody(physXTransform);

        PhysXCollider[] colliders = GetComponentsInChildren<PhysXCollider>(true);

        foreach (PhysXCollider collider in colliders) {
            collider.Setup(this, 0);
        }
    }

    public virtual void PostSceneInsertionSetup() {

    }

    public int AddCollider(PhysXCollider collider) {
        colliders.Add(collider.shape, collider);
        return PhysXLib.AttachShapeToRigidBody(collider.shape, physXBody);
    }

    public PhysXCollider GetColliderFromShape(IntPtr shape) {
        return colliders[shape];
    }

    public virtual void UpdatePositionAndVelocity() {
        
    }

    public void FireCollisionEvents(PhysXCollision collision) {
        if (collision.isEnter) {
            foreach (ICollisionEnterEvent collisionEnterEvent in collisionEnterEvents) {
                if (collisionEnterEvent.requiresData) collisionEnterEvent.CollisionEnter(collision);
                else collisionEnterEvent.CollisionEnter();
            }
        }
        if (collision.isStay) {
            foreach (ICollisionStayEvent collisionStayEvent in collisionStayEvents) {
                if (collisionStayEvent.requiresData) collisionStayEvent.CollisionStay(collision);
                else collisionStayEvent.CollisionStay();
            }
        }
        if (collision.isExit) {
            foreach (ICollisionExitEvent collisionExitEvent in collisionExitEvents) {
                if (collisionExitEvent.requiresData) collisionExitEvent.OnCollisionExit(collision);
                else collisionExitEvent.CollisionExit();
            }
        }
    }

    public void FireTriggerEvents(PhysXTrigger trigger) {
        if (trigger.isEnter) {
            foreach (ITriggerEnterEvent triggerEnterEvent in triggerEnterEvents) {
                if (triggerEnterEvent.requiresData) triggerEnterEvent.TriggerEnter(trigger.collider);
                else triggerEnterEvent.TriggerEnter();
            }
        }
        if (trigger.isExit) {
            foreach (ITriggerExitEvent triggerExitEvent in triggerExitEvents) {
                if (triggerExitEvent.requiresData) triggerExitEvent.TriggerExit(trigger.collider);
                else triggerExitEvent.TriggerExit();
            }
        }
    }

    protected virtual void OnDestroy() {
        PhysXLib.DestroyActor(physXBody);
        sceneManager.RemoveActor(this);
    }
}
