using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXCollider : MonoBehaviour
{
    //  TODO:   OnCollisionEnter stuff

    //  TODO:   ClosestPoint (maybe)

    //  TODO:   Triggers

    public PhysicMaterial material = null;

    public PhysXRigidBody attachedRigidbody { get; protected set; }

    private IntPtr _physXMaterial = IntPtr.Zero;
    protected IntPtr physXMaterial {
        get {
            if (_physXMaterial == IntPtr.Zero) {
                _physXMaterial = PhysXLib.CreateMaterial(material.staticFriction, material.dynamicFriction, material.bounciness);
            }

            return _physXMaterial;
        }
    }

    [HideInInspector]
    public IntPtr shape = IntPtr.Zero;

    // Start is called before the first frame update
    public virtual void Setup()
    {
        attachedRigidbody = GetComponentInParent<PhysXRigidBody>();
        attachedRigidbody.AddCollider(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}
