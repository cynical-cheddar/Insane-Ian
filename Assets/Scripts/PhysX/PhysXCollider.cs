using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXCollider : MonoBehaviour
{
    [Flags]
    public enum CollisionLayer {
        None = 0,
        Default = 1,
        Wheel   = (1 << 1),
        Obstacle   = (1 << 2),
        Player   = (1 << 3),
        Projectile   = (1 << 4),
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

    [SerializeField]
    private bool _trigger = false;
    public bool trigger {
        get {
            return _trigger;
        }
        set {
            _trigger = value;
        }
    }

    public int shapeNum { get; private set; }

    public Vector3 offset = Vector3.zero;

    // Start is called before the first frame update
    public virtual void Setup(PhysXBody attachedRigidBody, uint vehicleId)
    {
        Transform grandestParent = transform;
        while (grandestParent.parent != null) {
            grandestParent = grandestParent.parent;
        }

        PhysXVec3 position = new PhysXVec3(grandestParent.InverseTransformPoint(transform.TransformPoint(offset)));
        PhysXQuat rotation = new PhysXQuat(transform.rotation * Quaternion.Inverse(grandestParent.rotation));

        IntPtr localTransform = PhysXLib.CreateTransform(position, rotation);
        PhysXLib.SetShapeLocalTransform(shape, localTransform);

        PhysXLib.SetShapeSimulationFlag(shape, !trigger);
        PhysXLib.SetShapeSceneQueryFlag(shape, !trigger);
        PhysXLib.SetShapeTriggerFlag(shape, trigger);

        PhysXLib.CollisionEvent collisionEventFlags = attachedRigidBody.collisionEventFlags;
        PhysXLib.SetCollisionFilterData(shape, (UInt32)ownLayers, (UInt32)collisionLayers, (UInt32)collisionEventFlags, 0);
        PhysXLib.SetQueryFilterData(shape, (UInt32)ownLayers, 0, 0, vehicleId);
        shapeNum = attachedRigidBody.AddCollider(this);
    }
}
