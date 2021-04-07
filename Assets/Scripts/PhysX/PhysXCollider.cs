using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXCollider : MonoBehaviour
{
    [Flags]
    public enum CollisionLayer {
        None = 0,
        Default = 0b_0000_0000_0000_0001,
        Wheel   = 0b_0000_0000_0000_0010,
    }

    public CollisionLayer ownLayers = CollisionLayer.Default;

    public CollisionLayer collisionLayers = CollisionLayer.Default | CollisionLayer.Wheel;

    //  TODO:   ClosestPoint (maybe)

    //  TODO:   Triggers

    public PhysicMaterial material = null;

    public PhysXRigidBody attachedRigidbody { get; protected set; }

    private IntPtr _physXMaterial = IntPtr.Zero;
    protected IntPtr physXMaterial {
        get {
            if (_physXMaterial == IntPtr.Zero) {
                if (material == null) material = Instantiate(FindObjectOfType<PhysXSceneManager>().defaultMaterial);

                _physXMaterial = PhysXLib.CreateMaterial(material.staticFriction, material.dynamicFriction, material.bounciness);
            }

            return _physXMaterial;
        }
    }

    [HideInInspector]
    public IntPtr shape = IntPtr.Zero;

    public bool hasOnCollisionEnterEvent = false;

    // Start is called before the first frame update
    public virtual void Setup()
    {
        Transform grandestParent = transform;
        while (grandestParent.parent != null) {
            grandestParent = grandestParent.parent;
        }

        PhysXVec3 position = new PhysXVec3(grandestParent.InverseTransformPoint(transform.position));
        PhysXQuat rotation = new PhysXQuat(transform.rotation * Quaternion.Inverse(grandestParent.rotation));

        IntPtr localTransform = PhysXLib.CreateTransform(position, rotation);
        PhysXLib.SetShapeLocalTransform(shape, localTransform);

        //CollisionLayer ownCollisionLayers = CollisionLayer.None;
        //foreach (CollisionLayer collisionLayer in ownLayers)

        UInt32 collisionEventFlags = 0;
        if (hasOnCollisionEnterEvent) collisionEventFlags = 7;
        PhysXLib.SetCollisionFilterData(shape, (UInt32)ownLayers, (UInt32)collisionLayers, collisionEventFlags, 0);
        attachedRigidbody = GetComponentInParent<PhysXRigidBody>();
        attachedRigidbody.AddCollider(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}
