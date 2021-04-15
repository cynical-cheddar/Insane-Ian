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

    protected Vector3 _position;
    public Vector3 position {
        get {
            return _position;
        }
    }

    protected Quaternion _rotation;
    public Quaternion rotation {
        get {
            return _rotation;
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

    void Awake() {
        collisionEnterEvents = new List<ICollisionEnterEvent>(GetComponentsInChildren<ICollisionEnterEvent>(true));
        collisionStayEvents = new List<ICollisionStayEvent>(GetComponentsInChildren<ICollisionStayEvent>(true));
        collisionExitEvents = new List<ICollisionExitEvent>(GetComponentsInChildren<ICollisionExitEvent>(true));

        triggerEnterEvents = new List<ITriggerEnterEvent>(GetComponentsInChildren<ITriggerEnterEvent>(true));
        triggerExitEvents = new List<ITriggerExitEvent>(GetComponentsInChildren<ITriggerExitEvent>(true));

        sceneManager = FindObjectOfType<PhysXSceneManager>();

        sceneManager.AddActor(this);
    }

    public virtual void Setup() {
        IntPtr physXTransform = PhysXLib.CreateTransform(new PhysXVec3(transform.position), new PhysXQuat(transform.rotation));
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
                if (collisionEnterEvent.requiresData) collisionEnterEvent.OnCollisionEnter(collision);
                else collisionEnterEvent.OnCollisionEnter();
            }
        }
        if (collision.isStay) {
            foreach (ICollisionStayEvent collisionStayEvent in collisionStayEvents) {
                if (collisionStayEvent.requiresData) collisionStayEvent.OnCollisionStay(collision);
                else collisionStayEvent.OnCollisionStay();
            }
        }
        if (collision.isExit) {
            foreach (ICollisionExitEvent collisionExitEvent in collisionExitEvents) {
                if (collisionExitEvent.requiresData) collisionExitEvent.OnCollisionExit(collision);
                else collisionExitEvent.OnCollisionExit();
            }
        }
    }

    public void FireTriggerEvents(PhysXTrigger trigger) {
        if (trigger.isEnter) {
            foreach (ITriggerEnterEvent triggerEnterEvent in triggerEnterEvents) {
                if (triggerEnterEvent.requiresData) triggerEnterEvent.OnTriggerEnter(trigger.collider);
                else triggerEnterEvent.OnTriggerEnter();
            }
        }
        if (trigger.isExit) {
            foreach (ITriggerExitEvent triggerExitEvent in triggerExitEvents) {
                if (triggerExitEvent.requiresData) triggerExitEvent.OnTriggerExit(trigger.collider);
                else triggerExitEvent.OnTriggerExit();
            }
        }
    }
}
